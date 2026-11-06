using AS.Application.DTOs.Email;
using AS.Application.DTOs.Notificaciones;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Domain.Types.Torneo;
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
    // Los rivales ficticios de quien descansa
    private const int ID_USUARIO_BYE = 568;
    private const int ID_EQUIPO_BYE = 117;
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

    /// <summary>
    /// A cada inscrito, dónde juega y contra quién, para que no tenga que abrir la app justo al salir la ronda, cuando
    /// entran todos a la vez. Quien no tiene partida en la ronda recibe el aviso general
    /// </summary>
    public async Task AvisarRondaAsync(int idTorneo, int numeroRonda)
    {
        List<int> inscritos = await _dbamonsulContext.InscripcionTorneos
            .Where(i => i.IdTorneo == idTorneo)
            .Select(i => i.IdUsuario)
            .Distinct()
            .ToListAsync();

        await AvisarDeRondaAsync(
            idTorneo, numeroRonda, inscritos, $"Ronda {numeroRonda}", $"Ya está la ronda {numeroRonda}.");
    }

    /// <summary>
    /// A los jugadores indicados y a los miembros de los equipos indicados: lo que queda tras el cambio
    /// </summary>
    public async Task AvisarCambioEnRondaAsync(int idTorneo, int numeroRonda, List<int> idsUsuario, List<int> idsEquipo)
    {
        List<int> miembros = await _dbamonsulContext.InscripcionTorneos
            .Where(i => i.IdTorneo == idTorneo && i.IdEquipo != null && idsEquipo.Contains(i.IdEquipo.Value))
            .Select(i => i.IdUsuario)
            .ToListAsync();

        await AvisarDeRondaAsync(
            idTorneo,
            numeroRonda,
            [.. idsUsuario.Union(miembros)],
            $"Cambio en la ronda {numeroRonda}",
            $"Cambio en la ronda {numeroRonda}: mira el evento.");
    }

    private async Task AvisarDeRondaAsync(
        int idTorneo, int numeroRonda, List<int> idsUsuario, string encabezado, string sinDetalle)
    {
        var torneo = await _dbamonsulContext.Torneos
            .Where(t => t.IdTorneo == idTorneo)
            .Select(t => new { t.NombreTorneo, t.TipoTorneo, t.FormatoTorneo })
            .FirstOrDefaultAsync();
        // Ligas y copas van por jornadas, sin mesas, y tienen sus propios avisos
        if (torneo is null || FormatoTorneoType.EsPorJornadas(torneo.FormatoTorneo)) return;

        List<MovilDeUsuario> dispositivos = await MovilesDeAsync(idsUsuario);
        if (dispositivos.Count == 0) return;

        // En orden: la mesa de cada partida es su posición dentro de la ronda, como en la app
        List<MesaDeRonda> mesas = await _dbamonsulContext.PartidaTorneos
            .Where(p => p.IdTorneo == idTorneo && p.NumeroRonda == numeroRonda)
            .OrderBy(p => p.IdPartidaTorneo)
            .Select(p => new MesaDeRonda(p.IdUsuario1, p.IdUsuario2, p.IdEquipo1, p.IdEquipo2))
            .ToListAsync();

        Dictionary<int, string> detalles = torneo.TipoTorneo == TorneoType.INDIVIDUAL
            ? await DetallesIndividualesAsync(mesas)
            : await DetallesPorEquiposAsync(idTorneo, torneo.TipoTorneo, mesas);

        await EnviarPersonalesAsync(
            [.. dispositivos.Select(d => (
                d.Token,
                Cuerpo: detalles.TryGetValue(d.IdUsuario, out string? detalle) ? $"{encabezado} · {detalle}" : sinDetalle))],
            torneo.NombreTorneo,
            AvisoPush.RutaEvento(idTorneo));
    }

    /// <summary>
    /// A todos los jugadores de la partida, también a quien acaba de validar, con el marcador visto desde su lado
    /// </summary>
    public async Task AvisarPartidaValidadaAsync(int idPartidaTorneo)
    {
        var partida = await _dbamonsulContext.PartidaTorneos
            .Where(p => p.IdPartidaTorneo == idPartidaTorneo)
            .Select(p => new
            {
                p.IdTorneo,
                p.NumeroRonda,
                p.IdUsuario1,
                p.IdUsuario2,
                p.IdCompanero1,
                p.IdCompanero2,
                p.IdEquipo1,
                p.IdEquipo2,
                p.ResultadoUsuario1,
                p.ResultadoUsuario2,
            })
            .FirstOrDefaultAsync();
        if (partida is null || partida.IdUsuario1 == ID_USUARIO_BYE || partida.IdUsuario2 == ID_USUARIO_BYE) return;

        var torneo = await _dbamonsulContext.Torneos
            .Where(t => t.IdTorneo == partida.IdTorneo)
            .Select(t => new { t.NombreTorneo, t.TipoTorneo, t.FormatoTorneo })
            .FirstOrDefaultAsync();
        if (torneo is null) return;

        // En parejas juegan cuatro: el rival es la pareja. Si no, el jugador que ha tenido enfrente
        string rivalDel1;
        string rivalDel2;
        if (torneo.TipoTorneo == TorneoType.PAREJAS)
        {
            List<int> parejas = [.. new[] { partida.IdEquipo1, partida.IdEquipo2 }.OfType<int>()];
            Dictionary<int, string> nombres = await _dbamonsulContext.Equipo
                .Where(e => parejas.Contains(e.IdEquipo))
                .Select(e => new { e.IdEquipo, e.NombreEquipo })
                .ToDictionaryAsync(e => e.IdEquipo, e => e.NombreEquipo.Trim());
            rivalDel1 = nombres.GetValueOrDefault(partida.IdEquipo2 ?? 0, "tu rival");
            rivalDel2 = nombres.GetValueOrDefault(partida.IdEquipo1 ?? 0, "tu rival");
        }
        else
        {
            int[] jugadores = [partida.IdUsuario1, partida.IdUsuario2];
            Dictionary<int, string> nicks = await _dbamonsulContext.Usuarios
                .Where(u => jugadores.Contains(u.IdUsuario))
                .Select(u => new { u.IdUsuario, u.Nick })
                .ToDictionaryAsync(u => u.IdUsuario, u => u.Nick.Trim());
            rivalDel1 = nicks.GetValueOrDefault(partida.IdUsuario2, "tu rival");
            rivalDel2 = nicks.GetValueOrDefault(partida.IdUsuario1, "tu rival");
        }

        // Ligas y copas van por jornadas y fases, sin el número de ronda de un torneo
        string encabezado = !FormatoTorneoType.EsPorJornadas(torneo.FormatoTorneo) && partida.NumeroRonda is int ronda
            ? $"Ronda {ronda} · "
            : "";
        string Cuerpo(int? propio, int? delRival, string rival) => propio is int p && delRival is int r
            ? $"{encabezado}Partida validada: {p}-{r} contra {rival}"
            : $"{encabezado}Partida validada contra {rival}";

        Dictionary<int, string> cuerpos = [];
        string cuerpo1 = Cuerpo(partida.ResultadoUsuario1, partida.ResultadoUsuario2, rivalDel1);
        string cuerpo2 = Cuerpo(partida.ResultadoUsuario2, partida.ResultadoUsuario1, rivalDel2);
        cuerpos.TryAdd(partida.IdUsuario1, cuerpo1);
        cuerpos.TryAdd(partida.IdUsuario2, cuerpo2);
        if (partida.IdCompanero1 is int companero1) cuerpos.TryAdd(companero1, cuerpo1);
        if (partida.IdCompanero2 is int companero2) cuerpos.TryAdd(companero2, cuerpo2);

        List<MovilDeUsuario> moviles = await MovilesDeAsync([.. cuerpos.Keys]);
        await EnviarPersonalesAsync(
            [.. moviles.Select(m => (m.Token, Cuerpo: cuerpos[m.IdUsuario]))],
            torneo.NombreTorneo,
            AvisoPush.RutaEvento(partida.IdTorneo));
    }

    private async Task<List<MovilDeUsuario>> MovilesDeAsync(List<int> idsUsuario) => await (
        from dispositivo in _dbamonsulContext.DispositivosPush
        join usuario in _dbamonsulContext.Usuarios on dispositivo.IdUsuario equals usuario.IdUsuario
        where idsUsuario.Contains(dispositivo.IdUsuario) && usuario.PermiteNotificaciones
        select new MovilDeUsuario(dispositivo.IdUsuario, dispositivo.Token)).ToListAsync();

    private async Task<Dictionary<int, string>> DetallesIndividualesAsync(List<MesaDeRonda> mesas)
    {
        List<int> jugadores = [.. mesas.SelectMany(m => new[] { m.IdUsuario1, m.IdUsuario2 }).Distinct()];
        Dictionary<int, string> nicks = await _dbamonsulContext.Usuarios
            .Where(u => jugadores.Contains(u.IdUsuario))
            .Select(u => new { u.IdUsuario, u.Nick })
            .ToDictionaryAsync(u => u.IdUsuario, u => u.Nick.Trim());

        string Detalle(int rival, int numeroMesa) => rival == ID_USUARIO_BYE
            ? "Descansas esta ronda"
            : $"Mesa {numeroMesa} · contra {nicks.GetValueOrDefault(rival, "tu rival")}";

        Dictionary<int, string> detalles = [];
        for (int i = 0; i < mesas.Count; i++)
        {
            detalles.TryAdd(mesas[i].IdUsuario1, Detalle(mesas[i].IdUsuario2, i + 1));
            detalles.TryAdd(mesas[i].IdUsuario2, Detalle(mesas[i].IdUsuario1, i + 1));
        }
        return detalles;
    }

    // Sin el jugador ni su mesa exacta: el capitán puede moverlos entre las mesas de su equipo, y el aviso seguiría valiendo
    private async Task<Dictionary<int, string>> DetallesPorEquiposAsync(
        int idTorneo, string tipoTorneo, List<MesaDeRonda> mesas)
    {
        var lados = mesas
            .SelectMany((m, i) => new[]
            {
                (Equipo: m.IdEquipo1, Rival: m.IdEquipo2, Mesa: i + 1),
                (Equipo: m.IdEquipo2, Rival: m.IdEquipo1, Mesa: i + 1),
            })
            .Where(l => l.Equipo is int equipo && equipo != ID_EQUIPO_BYE && l.Rival is not null)
            .GroupBy(l => l.Equipo!.Value)
            .ToList();
        List<int> idsEquipo = [.. mesas.SelectMany(m => new[] { m.IdEquipo1, m.IdEquipo2 }).OfType<int>().Distinct()];

        Dictionary<int, string> nombres = await _dbamonsulContext.Equipo
            .Where(e => idsEquipo.Contains(e.IdEquipo))
            .Select(e => new { e.IdEquipo, e.NombreEquipo })
            .ToDictionaryAsync(e => e.IdEquipo, e => e.NombreEquipo.Trim());
        var miembros = await _dbamonsulContext.InscripcionTorneos
            .Where(i => i.IdTorneo == idTorneo && i.IdEquipo != null && idsEquipo.Contains(i.IdEquipo.Value))
            .Select(i => new { i.IdUsuario, IdEquipo = i.IdEquipo!.Value })
            .ToListAsync();

        string descanso = tipoTorneo == TorneoType.PAREJAS
            ? "Tu pareja descansa esta ronda"
            : "Tu equipo descansa esta ronda";
        Dictionary<int, string> porEquipo = lados.ToDictionary(
            g => g.Key,
            g => g.First().Rival == ID_EQUIPO_BYE
                ? descanso
                : $"{TextoMesas([.. g.Select(l => l.Mesa)])} · contra {nombres.GetValueOrDefault(g.First().Rival!.Value, "tu rival")}");

        Dictionary<int, string> detalles = [];
        foreach (var miembro in miembros)
            if (porEquipo.TryGetValue(miembro.IdEquipo, out string? detalle))
                detalles.TryAdd(miembro.IdUsuario, detalle);
        return detalles;
    }

    private static string TextoMesas(List<int> mesas) => mesas.Count switch
    {
        1 => $"Mesa {mesas[0]}",
        2 => $"Mesas {mesas[0]} y {mesas[1]}",
        _ => $"Mesas {mesas[0]} a {mesas[^1]}",
    };

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
                await TratarFallosAsync(respuesta, lote, errores);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudieron enviar las notificaciones");
            errores.Add(ex.GetType().Name);
        }

        return new(tokens.Count, entregados, [.. errores.Distinct()]);
    }

    // Un texto distinto para cada móvil. Igual que arriba: un fallo aquí se registra y se sigue
    private async Task EnviarPersonalesAsync(List<(string Token, string Cuerpo)> avisos, string titulo, string ruta)
    {
        List<string> errores = [];
        try
        {
            FirebaseMessaging? mensajeria = Mensajeria();
            if (mensajeria is null)
            {
                _logger.LogWarning("Sin credenciales de Firebase: no se envían notificaciones");
                return;
            }

            foreach ((string Token, string Cuerpo)[] lote in avisos.Chunk(TOKENS_POR_ENVIO))
            {
                BatchResponse respuesta = await mensajeria.SendEachAsync(
                    lote.Select(a => MensajeAUnMovil(a.Token, titulo, a.Cuerpo, ruta)));
                await TratarFallosAsync(respuesta, [.. lote.Select(a => a.Token)], errores);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudieron enviar las notificaciones");
            return;
        }

        if (errores.Count > 0)
            _logger.LogWarning("Fallos al enviar los avisos: {Errores}", string.Join(", ", errores.Distinct()));
    }

    private async Task TratarFallosAsync(BatchResponse respuesta, string[] tokens, List<string> errores)
    {
        List<string> caducados = [];
        for (int i = 0; i < respuesta.Responses.Count; i++)
        {
            MessagingErrorCode? codigo = respuesta.Responses[i].Exception?.MessagingErrorCode;
            if (respuesta.Responses[i].IsSuccess) continue;
            if (codigo is MessagingErrorCode.Unregistered or MessagingErrorCode.SenderIdMismatch)
                caducados.Add(tokens[i]);
            else
                errores.Add(codigo?.ToString() ?? "Desconocido");
        }

        // Se ha desinstalado la app: ese token ya no vale
        if (caducados.Count > 0)
            await _dbamonsulContext.DispositivosPush
                .Where(d => caducados.Contains(d.Token))
                .ExecuteDeleteAsync();
    }

    private static Message MensajeAUnMovil(string token, string titulo, string cuerpo, string ruta) => new()
    {
        Token = token,
        Notification = new Notification { Title = titulo, Body = cuerpo },
        Data = new Dictionary<string, string> { ["ruta"] = ruta },
        Android = new AndroidConfig { Priority = Priority.High },
        Apns = new ApnsConfig { Aps = new Aps { Sound = "default" } },
    };

    private sealed record MesaDeRonda(int IdUsuario1, int IdUsuario2, int? IdEquipo1, int? IdEquipo2);
    private sealed record MovilDeUsuario(int IdUsuario, string Token);

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
