using AS.Application.Interfaces;
using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;
using AS.Domain.Types.Torneo;
using AS.Infrastructure.Repositories.Interfaces;
using AS.Utils.Statics;

namespace AS.Application.Services;

/// <summary>
/// Torneos con formato Liga: generar las jornadas de todos contra todos, rehacerlas y retirar jugadores.
/// El calendario sale de RoundRobin, que también servirá para los grupos de otros formatos
/// </summary>
public class LigaApplication(IUnitOfWork unitOfWork, IEmailApplicacion emailApplicacion) : ILigaApplication
{
    public const string ESTADO_RETIRADO = "RETIRADO";
    private const int MINIMO_JUGADORES = 3;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEmailApplicacion _emailApplicacion = emailApplicacion;

    public async Task<(bool Ok, string? Error)> GenerarAsync(int idTorneo)
    {
        Torneo? torneo = await GetLigaAsync(idTorneo);
        if (torneo is null)
            return (false, "El torneo no es una liga.");
        if (await TienePartidasAsync(idTorneo))
            return (false, "La liga ya está generada.");

        List<InscripcionTorneo> inscripciones =
        [
            .. (await _unitOfWork.InscripcionRepository.GetInscripcionesByTorneo(idTorneo)).Where(i =>
                i.EstadoInscripcion != ESTADO_RETIRADO
            ),
        ];
        if (inscripciones.Count < MINIMO_JUGADORES)
            return (false, $"Hacen falta al menos {MINIMO_JUGADORES} jugadores para generar la liga.");

        // El sorteo del orden decide quién se enfrenta a quién en cada jornada
        List<InscripcionTorneo> sorteo = [.. inscripciones.OrderBy(_ => Random.Shared.Next())];
        List<List<(InscripcionTorneo Local, InscripcionTorneo Visitante)>> jornadas =
            RoundRobin.GenerarJornadas(sorteo, torneo.IdaYVuelta);

        List<PartidaTorneo> partidas = [];
        for (int jornada = 0; jornada < jornadas.Count; jornada++)
            foreach ((InscripcionTorneo local, InscripcionTorneo visitante) in jornadas[jornada])
                partidas.Add(
                    new PartidaTorneo
                    {
                        IdTorneo = idTorneo,
                        NumeroRonda = jornada + 1,
                        IdUsuario1 = local.IdUsuario,
                        IdUsuario2 = visitante.IdUsuario,
                        // Cada jugador lleva toda la liga el ejército que eligió al inscribirse
                        EjercitoUsuario1 = local.EjercitoElegido,
                        EjercitoUsuario2 = visitante.EjercitoElegido,
                        FechaPartida = torneo.FechaInicioTorneo,
                        EsElo = torneo.CuentaElo,
                        EsMatchedPlayPartida = torneo.EsMatchedPlayTorneo,
                        PuntosPartida = torneo.PuntosTorneo,
                        EscenarioPartida = "",
                        PartidaValidadaUsuario1 = false,
                        PartidaValidadaUsuario2 = false,
                    }
                );

        await _unitOfWork.PartidaTorneoRepository.RegisterMany(partidas);

        // Las rondas del torneo son las jornadas: así las vistas saben cuántas pestañas pintar
        torneo.NumeroPartidas = jornadas.Count;
        await _unitOfWork.TorneoRepository.Edit(torneo);

        List<string> destinatarios =
        [
            .. inscripciones.Select(i => i.IdUsuarioNavigation?.Email).OfType<string>(),
        ];
        if (destinatarios.Count > 0)
            _emailApplicacion.SendEmailLigaGenerada(torneo.NombreTorneo, jornadas.Count, destinatarios);

        return (true, null);
    }

    /// <summary>
    /// Deshace la liga generada para volver a sortearla, solo si aún no se ha jugado nada
    /// </summary>
    public async Task<(bool Ok, string? Error)> EliminarJornadasAsync(int idTorneo)
    {
        Torneo? torneo = await GetLigaAsync(idTorneo);
        if (torneo is null)
            return (false, "El torneo no es una liga.");
        if (await _unitOfWork.GanadorRepository.ExistsByTorneoAsync(idTorneo))
            return (false, "La liga ya está cerrada: tiene los resultados guardados.");

        List<PartidaTorneoDTO> partidas = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneoAsync(
            idTorneo
        );
        if (partidas.Count == 0)
            return (false, "La liga no tiene jornadas generadas.");
        if (partidas.Any(TieneResultado))
            return (false, "Ya hay partidas con resultado: la liga no se puede eliminar.");

        await _unitOfWork.PartidaTorneoRepository.DeletePartidasAsync(idTorneo);
        torneo.NumeroPartidas = 1;
        await _unitOfWork.TorneoRepository.Edit(torneo);
        return (true, null);
    }

    /// <summary>
    /// Borra todas las partidas del jugador, también las jugadas, para que no cuenten en la clasificación ni en el
    /// ELO, y deja su inscripción como retirada
    /// </summary>
    public async Task<(bool Ok, string? Error)> RetirarJugadorAsync(int idTorneo, int idInscripcion)
    {
        if (await GetLigaAsync(idTorneo) is null)
            return (false, "El torneo no es una liga.");

        InscripcionTorneo? inscripcion = await _unitOfWork.InscripcionRepository.GetInscripcionById(
            idInscripcion
        );
        if (inscripcion is null || inscripcion.IdTorneo != idTorneo)
            return (false, "La inscripción no es de esta liga.");
        if (inscripcion.EstadoInscripcion == ESTADO_RETIRADO)
            return (false, "El jugador ya está retirado.");
        if (!await TienePartidasAsync(idTorneo))
            return (false, "La liga aún no está generada: quita la inscripción en su lugar.");
        if (await _unitOfWork.GanadorRepository.ExistsByTorneoAsync(idTorneo))
            return (false, "La liga ya está cerrada: tiene los resultados guardados.");

        await _unitOfWork.PartidaTorneoRepository.DeletePartidasAsync(idTorneo, inscripcion.IdUsuario);
        await _unitOfWork.InscripcionRepository.ActualizarEstadoInscripcionAsync(idInscripcion, ESTADO_RETIRADO);
        return (true, null);
    }

    private async Task<Torneo?> GetLigaAsync(int idTorneo)
    {
        Torneo? torneo = await _unitOfWork.TorneoRepository.GetById(idTorneo);
        return torneo?.FormatoTorneo == FormatoTorneoType.LIGA ? torneo : null;
    }

    private async Task<bool> TienePartidasAsync(int idTorneo) =>
        (await _unitOfWork.PartidaTorneoRepository.GetNumerosRondasConPartidasAsync(idTorneo)).Count > 0;

    private static bool TieneResultado(PartidaTorneoDTO partida) =>
        partida.ResultadoUsuario1 is not null
        || partida.ResultadoUsuario2 is not null
        || partida.PartidaValidadaUsuario1 == true
        || partida.PartidaValidadaUsuario2 == true;
}
