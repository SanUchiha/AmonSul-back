using AS.Application.DTOs.Email;
using AS.Application.DTOs.Notificaciones;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Data;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AS.Application.Services;

public class NotificacionPushApplication(
    DbamonsulContext dbamonsulContext,
    IConfiguration configuration,
    IOptions<EmailSettings> emailSettings,
    ILogger<NotificacionPushApplication> logger) : INotificacionPushApplication
{
    // El máximo de Firebase por envío
    private const int TOKENS_POR_ENVIO = 500;
    // Los eventos sin hora se avisan a esta. Con la web dormida el aviso sale al despertar, pero no pasado este margen
    private static readonly TimeOnly HORA_INICIO_POR_DEFECTO = new(9, 0);
    private const int HORAS_MARGEN_INICIO = 3;
    private const string ESTADO_CANCELADO = "CANCELADO";
    private static readonly object _cerrojoFirebase = new();

    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;
    private readonly IConfiguration _configuration = configuration;
    private readonly List<string> _emailsExcluidos = emailSettings.Value.EmailsExcluidos;
    private readonly ILogger<NotificacionPushApplication> _logger = logger;

    // El token es del móvil, no de la cuenta: si otra cuenta entró antes en este móvil, pasa a esta
    public async Task RegistrarDispositivoAsync(int idUsuario, string token, string plataforma)
    {
        await _dbamonsulContext.DispositivosPush
            .Where(d => d.Token == token && d.IdUsuario != idUsuario)
            .ExecuteDeleteAsync();

        if (await _dbamonsulContext.DispositivosPush.AnyAsync(d => d.Token == token))
            return;

        _dbamonsulContext.DispositivosPush.Add(new DispositivoPush
        {
            IdUsuario = idUsuario,
            Token = token,
            Plataforma = plataforma,
            FechaCreacion = DateTime.UtcNow,
        });

        try
        {
            await _dbamonsulContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // La app lo ha registrado dos veces a la vez: con uno basta
            _dbamonsulContext.ChangeTracker.Clear();
        }
    }

    public async Task QuitarDispositivoAsync(int idUsuario, string token) =>
        await _dbamonsulContext.DispositivosPush
            .Where(d => d.IdUsuario == idUsuario && d.Token == token)
            .ExecuteDeleteAsync();

    public async Task<ResultadoPushDTO> EnviarAUsuariosAsync(
        IEnumerable<int> idsUsuario, string titulo, string cuerpo, string? ruta = null)
    {
        List<int> ids = [.. idsUsuario.Distinct()];
        List<string> tokens = await (
            from dispositivo in _dbamonsulContext.DispositivosPush
            join usuario in _dbamonsulContext.Usuarios on dispositivo.IdUsuario equals usuario.IdUsuario
            where ids.Contains(dispositivo.IdUsuario) && usuario.PermiteNotificaciones
            select dispositivo.Token).ToListAsync();

        return await EnviarAsync(tokens, titulo, cuerpo, ruta);
    }

    public async Task EnviarAEmailsAsync(List<string> emails, string titulo, string cuerpo, string? ruta)
    {
        List<string> tokens = await (
            from dispositivo in _dbamonsulContext.DispositivosPush
            join usuario in _dbamonsulContext.Usuarios on dispositivo.IdUsuario equals usuario.IdUsuario
            where emails.Contains(usuario.Email) && usuario.PermiteNotificaciones
            select dispositivo.Token).ToListAsync();

        await EnviarAsync(tokens, titulo, cuerpo, ruta);
    }

    public async Task EnviarATodosAsync(string titulo, string cuerpo, string? ruta)
    {
        List<string> tokens = await (
            from dispositivo in _dbamonsulContext.DispositivosPush
            join usuario in _dbamonsulContext.Usuarios on dispositivo.IdUsuario equals usuario.IdUsuario
            where usuario.PermiteNotificaciones && !_emailsExcluidos.Contains(usuario.Email)
            select dispositivo.Token).ToListAsync();

        await EnviarAsync(tokens, titulo, cuerpo, ruta);
    }

    public async Task AvisarEventoTerminadoAsync(int idTorneo)
    {
        var torneo = await EventosParaTodos()
            .Where(t => t.IdTorneo == idTorneo)
            .Select(t => new { t.NombreTorneo })
            .FirstOrDefaultAsync();
        if (torneo is null) return;

        await EnviarATodosAsync(
            "Evento terminado",
            $"{torneo.NombreTorneo} ha terminado. Ya puedes ver la clasificación.",
            AvisoPush.RutaEvento(idTorneo));
    }

    // Cada pocos minutos: los eventos de hoy que ya han llegado a su hora y no se han avisado
    public async Task AvisarEventosQueEmpiezanAsync()
    {
        DateTime ahora = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time"));
        DateOnly hoy = DateOnly.FromDateTime(ahora);

        var eventos = await EventosParaTodos()
            .Where(t => t.FechaInicioTorneo == hoy && !t.AvisoInicioEnviado)
            .Select(t => new { t.IdTorneo, t.NombreTorneo, t.HoraInicioTorneo })
            .ToListAsync();

        foreach (var evento in eventos)
        {
            DateTime inicio = hoy.ToDateTime(evento.HoraInicioTorneo ?? HORA_INICIO_POR_DEFECTO);
            if (ahora < inicio || ahora > inicio.AddHours(HORAS_MARGEN_INICIO)) continue;

            // Marcado antes de enviar: mejor perder un aviso que mandarlo dos veces a todo el mundo
            int marcados = await _dbamonsulContext.Torneos
                .Where(t => t.IdTorneo == evento.IdTorneo && !t.AvisoInicioEnviado)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.AvisoInicioEnviado, true));
            if (marcados == 0) continue;

            await EnviarATodosAsync(
                "Empieza un evento",
                $"Hoy empieza {evento.NombreTorneo}.",
                AvisoPush.RutaEvento(evento.IdTorneo));
        }
    }

    // Los de prueba y los privados no se anuncian, como ya pasaba con el correo de nuevo torneo
    private IQueryable<Torneo> EventosParaTodos() =>
        _dbamonsulContext.Torneos.Where(t =>
            !t.EsPrivadoTorneo && t.EstadoTorneo != ESTADO_CANCELADO && !t.NombreTorneo.Contains("test"));

    // Un fallo aquí nunca debe romper lo que lo ha provocado: se registra y se sigue
    private async Task<ResultadoPushDTO> EnviarAsync(List<string> tokens, string titulo, string cuerpo, string? ruta)
    {
        if (tokens.Count == 0) return new(0, 0, []);

        List<string> errores = [];
        int entregados = 0;
        try
        {
            FirebaseMessaging? mensajeria = Mensajeria();
            if (mensajeria is null)
            {
                _logger.LogWarning("Sin credenciales de Firebase: no se envían notificaciones");
                return new(tokens.Count, 0, ["Sin credenciales de Firebase"]);
            }

            foreach (string[] lote in tokens.Chunk(TOKENS_POR_ENVIO))
            {
                BatchResponse respuesta = await mensajeria.SendEachForMulticastAsync(
                    Mensaje(lote, titulo, cuerpo, ruta));
                entregados += respuesta.SuccessCount;

                List<string> caducados = [];
                for (int i = 0; i < respuesta.Responses.Count; i++)
                {
                    MessagingErrorCode? codigo = respuesta.Responses[i].Exception?.MessagingErrorCode;
                    if (respuesta.Responses[i].IsSuccess) continue;
                    if (codigo is MessagingErrorCode.Unregistered or MessagingErrorCode.SenderIdMismatch)
                        caducados.Add(lote[i]);
                    else
                        errores.Add(codigo?.ToString() ?? "Desconocido");
                }

                // Se ha desinstalado la app: ese token ya no vale
                if (caducados.Count > 0)
                    await _dbamonsulContext.DispositivosPush
                        .Where(d => caducados.Contains(d.Token))
                        .ExecuteDeleteAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudieron enviar las notificaciones");
            errores.Add(ex.GetType().Name);
        }

        return new(tokens.Count, entregados, [.. errores.Distinct()]);
    }

    private static MulticastMessage Mensaje(string[] tokens, string titulo, string cuerpo, string? ruta) => new()
    {
        Tokens = tokens,
        Notification = new Notification { Title = titulo, Body = cuerpo },
        // La ruta a la que lleva pulsar el aviso
        Data = ruta is null ? null : new Dictionary<string, string> { ["ruta"] = ruta },
        Android = new AndroidConfig { Priority = Priority.High },
        Apns = new ApnsConfig { Aps = new Aps { Sound = "default" } },
    };

    // Una sola instancia por proceso. Null si falta la clave, que no está en el repo: la pone el despliegue
    private FirebaseMessaging? Mensajeria()
    {
        lock (_cerrojoFirebase)
        {
            if (FirebaseApp.DefaultInstance is null)
            {
                string? credenciales = _configuration["Firebase:CredentialsJson"];
                if (string.IsNullOrWhiteSpace(credenciales)) return null;
                FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromJson(credenciales) });
            }
            return FirebaseMessaging.DefaultInstance;
        }
    }
}
