using AS.Application.Interfaces;
using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;
using AS.Domain.Types.Torneo;
using AS.Infrastructure.Repositories.Interfaces;
using AS.Utils.Statics;

namespace AS.Application.Services;

/// <summary>
/// Torneos con formato Copa, fase de grupos: repartir a los jugadores en grupos (por ELO con bombos, por sorteo o a
/// mano), generar las jornadas de cada grupo, todos contra todos como en la liga, rehacerlas y retirar jugadores
/// </summary>
public class CopaApplication(
    IUnitOfWork unitOfWork,
    IEmailApplicacion emailApplicacion,
    IEloLigaApplication eloLigaApplication
) : ICopaApplication
{
    private const int MINIMO_POR_GRUPO = 2;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEmailApplicacion _emailApplicacion = emailApplicacion;
    private readonly IEloLigaApplication _eloLigaApplication = eloLigaApplication;

    /// <summary>
    /// Reparte a todos con el método de la copa. Por ELO, los mejores van uno a cada grupo como cabezas de serie y el
    /// resto por bombos; por sorteo, al azar. En los dos los grupos quedan iguales o con uno de diferencia. Se puede
    /// repetir hasta generar las jornadas, y luego mover a quien haga falta a mano
    /// </summary>
    public async Task<(bool Ok, string? Error)> RepartirGruposAsync(int idTorneo)
    {
        Torneo? copa = await GetCopaAsync(idTorneo);
        if (copa is null)
            return (false, "El torneo no es una copa.");
        if (await TienePartidasAsync(idTorneo))
            return (false, "Los grupos ya tienen jornadas: elimínalas para volver a repartir.");
        if (copa.RepartoGrupos == RepartoGruposType.MANUAL)
            return (false, "El reparto es a mano: coloca a cada jugador en su grupo.");

        int grupos = copa.NumeroGrupos!.Value;
        List<InscripcionTorneo> jugadores = await JugadoresAsync(idTorneo);
        if (jugadores.Count < grupos * MINIMO_POR_GRUPO)
            return (false, $"Hacen falta al menos {grupos * MINIMO_POR_GRUPO} jugadores para {grupos} grupos.");

        List<InscripcionTorneo> orden =
            copa.RepartoGrupos == RepartoGruposType.ELO
                ? await PorEloAsync(jugadores)
                : [.. jugadores.OrderBy(_ => Random.Shared.Next())];

        // Cada bombo da uno a cada grupo, en un orden de grupos al azar: el último, si no está lleno, cae en grupos
        // distintos cada vez
        Dictionary<int, int> grupoPorInscripcion = [];
        foreach (InscripcionTorneo[] bombo in orden.Chunk(grupos))
        {
            List<int> ordenGrupos = [.. Enumerable.Range(1, grupos).OrderBy(_ => Random.Shared.Next())];
            List<InscripcionTorneo> sorteo = [.. bombo.OrderBy(_ => Random.Shared.Next())];
            for (int i = 0; i < sorteo.Count; i++)
                grupoPorInscripcion[sorteo[i].IdInscripcion] = ordenGrupos[i];
        }

        await _unitOfWork.InscripcionRepository.ActualizarGruposAsync(grupoPorInscripcion);
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> CambiarGrupoAsync(int idTorneo, int idInscripcion, int grupo)
    {
        Torneo? copa = await GetCopaAsync(idTorneo);
        if (copa is null)
            return (false, "El torneo no es una copa.");
        if (await TienePartidasAsync(idTorneo))
            return (false, "Los grupos ya tienen jornadas: elimínalas para cambiar a alguien de grupo.");
        if (grupo < 1 || grupo > copa.NumeroGrupos)
            return (false, $"El grupo tiene que ser del 1 al {copa.NumeroGrupos}.");

        InscripcionTorneo? inscripcion = (await JugadoresAsync(idTorneo)).FirstOrDefault(i =>
            i.IdInscripcion == idInscripcion
        );
        if (inscripcion is null)
            return (false, "La inscripción no es de esta copa.");

        await _unitOfWork.InscripcionRepository.ActualizarGruposAsync(
            new Dictionary<int, int> { [idInscripcion] = grupo }
        );
        return (true, null);
    }

    /// <summary>
    /// Todas las jornadas de cada grupo, todos contra todos. La jornada 1 de todos los grupos es la ronda 1, y así;
    /// un grupo más pequeño acaba antes
    /// </summary>
    public async Task<(bool Ok, string? Error)> GenerarGruposAsync(int idTorneo)
    {
        Torneo? copa = await GetCopaAsync(idTorneo);
        if (copa is null)
            return (false, "El torneo no es una copa.");
        if (await TienePartidasAsync(idTorneo))
            return (false, "Los grupos ya tienen jornadas.");

        int grupos = copa.NumeroGrupos!.Value;
        List<InscripcionTorneo> jugadores = await JugadoresAsync(idTorneo);
        List<InscripcionTorneo> sinGrupo = [.. jugadores.Where(i => i.Grupo is null || i.Grupo < 1 || i.Grupo > grupos)];
        if (sinGrupo.Count > 0)
            return (false, $"Faltan por colocar en un grupo: {string.Join(", ", sinGrupo.Select(Nick))}.");

        List<List<InscripcionTorneo>> porGrupo =
        [
            .. Enumerable.Range(1, grupos).Select(g => jugadores.Where(i => i.Grupo == g).ToList()),
        ];
        for (int g = 0; g < grupos; g++)
            if (porGrupo[g].Count < MINIMO_POR_GRUPO)
                return (false, $"El grupo {g + 1} necesita al menos {MINIMO_POR_GRUPO} jugadores.");
        int menor = porGrupo.Min(g => g.Count);
        if (menor <= copa.ClasificadosPorGrupo)
            return (
                false,
                $"Con un grupo de {menor} no pueden pasar {copa.ClasificadosPorGrupo} de cada uno: deja menos clasificados o haz menos grupos."
            );

        List<PartidaTorneo> partidas = [];
        int jornadas = 0;
        for (int g = 0; g < grupos; g++)
        {
            // El sorteo del orden decide quién se enfrenta a quién en cada jornada
            List<InscripcionTorneo> sorteo = [.. porGrupo[g].OrderBy(_ => Random.Shared.Next())];
            List<List<(InscripcionTorneo Local, InscripcionTorneo Visitante)>> calendario =
                RoundRobin.GenerarJornadas(sorteo, copa.IdaYVuelta);
            jornadas = Math.Max(jornadas, calendario.Count);
            for (int jornada = 0; jornada < calendario.Count; jornada++)
                foreach ((InscripcionTorneo local, InscripcionTorneo visitante) in calendario[jornada])
                {
                    PartidaTorneo partida = LigaApplication.NuevaPartida(copa, jornada + 1, local, visitante);
                    partida.Fase = FaseCopaType.GRUPOS;
                    partida.Grupo = g + 1;
                    partidas.Add(partida);
                }
        }

        await _unitOfWork.PartidaTorneoRepository.RegisterMany(partidas);

        // Las rondas del torneo son las jornadas de grupos: así las vistas saben cuántas pintar
        copa.NumeroPartidas = jornadas;
        await _unitOfWork.TorneoRepository.Edit(copa);

        List<string> destinatarios = [.. jugadores.Select(i => i.IdUsuarioNavigation?.Email).OfType<string>()];
        if (destinatarios.Count > 0)
            _emailApplicacion.SendEmailCopaGenerada(
                copa.NombreTorneo,
                jornadas,
                copa.ClasificadosPorGrupo!.Value,
                destinatarios
            );

        return (true, null);
    }

    /// <summary>
    /// Deshace las jornadas de grupos para volver a repartir o sortear, solo si aún no se ha jugado nada. Los
    /// jugadores se quedan en su grupo
    /// </summary>
    public async Task<(bool Ok, string? Error)> EliminarJornadasAsync(int idTorneo)
    {
        Torneo? copa = await GetCopaAsync(idTorneo);
        if (copa is null)
            return (false, "El torneo no es una copa.");

        List<PartidaTorneoDTO> partidas = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneoAsync(idTorneo);
        if (partidas.Count == 0)
            return (false, "Los grupos no tienen jornadas generadas.");
        if (partidas.Any(p => p.Fase != FaseCopaType.GRUPOS))
            return (false, "La eliminatoria ya ha empezado: los grupos no se pueden eliminar.");
        if (partidas.Any(TieneResultado))
            return (false, "Ya hay partidas con resultado: los grupos no se pueden eliminar.");

        await _unitOfWork.PartidaTorneoRepository.DeletePartidasAsync(idTorneo);
        copa.NumeroPartidas = 1;
        await _unitOfWork.TorneoRepository.Edit(copa);
        return (true, null);
    }

    /// <summary>
    /// En la fase de grupos, como en la liga: se quita el ELO de sus partidas que ya contaban y se borran todas, también
    /// las jugadas, para que no cuenten en la clasificación del grupo
    /// </summary>
    public async Task<(bool Ok, string? Error)> RetirarJugadorAsync(int idTorneo, int idInscripcion)
    {
        if (await GetCopaAsync(idTorneo) is null)
            return (false, "El torneo no es una copa.");

        InscripcionTorneo? inscripcion = await _unitOfWork.InscripcionRepository.GetInscripcionById(idInscripcion);
        if (inscripcion is null || inscripcion.IdTorneo != idTorneo)
            return (false, "La inscripción no es de esta copa.");
        if (inscripcion.EstadoInscripcion == LigaApplication.ESTADO_RETIRADO)
            return (false, "El jugador ya está retirado.");
        if (!await TienePartidasAsync(idTorneo))
            return (false, "Los grupos aún no tienen jornadas: quita la inscripción en su lugar.");
        if (await _unitOfWork.GanadorRepository.ExistsByTorneoAsync(idTorneo))
            return (false, "La copa ya está cerrada: tiene los resultados guardados.");
        List<PartidaTorneoDTO> partidas = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneoAsync(idTorneo);
        if (partidas.Any(p => p.Fase != FaseCopaType.GRUPOS))
            return (false, "La eliminatoria ya ha empezado: un jugador no se puede retirar todavía en ella.");

        await _unitOfWork.EnTransaccionAsync(async () =>
        {
            await _eloLigaApplication.QuitarDeJugadorAsync(idTorneo, inscripcion.IdUsuario);
            await _unitOfWork.InscripcionRepository.RetirarAsync(idInscripcion, LigaApplication.ESTADO_RETIRADO);
        });
        return (true, null);
    }

    private async Task<Torneo?> GetCopaAsync(int idTorneo)
    {
        Torneo? torneo = await _unitOfWork.TorneoRepository.GetById(idTorneo);
        return torneo?.FormatoTorneo == FormatoTorneoType.COPA ? torneo : null;
    }

    private async Task<bool> TienePartidasAsync(int idTorneo) =>
        (await _unitOfWork.PartidaTorneoRepository.GetNumerosRondasConPartidasAsync(idTorneo)).Count > 0;

    // Los que juegan: los retirados ya no cuentan
    private async Task<List<InscripcionTorneo>> JugadoresAsync(int idTorneo) =>
    [
        .. (await _unitOfWork.InscripcionRepository.GetInscripcionesByTorneo(idTorneo)).Where(i =>
            i.EstadoInscripcion != LigaApplication.ESTADO_RETIRADO
        ),
    ];

    // De más a menos ELO; los empatados, al azar
    private async Task<List<InscripcionTorneo>> PorEloAsync(List<InscripcionTorneo> jugadores)
    {
        Dictionary<int, int> elo = [];
        foreach (InscripcionTorneo jugador in jugadores)
            elo[jugador.IdInscripcion] = await _unitOfWork.EloRepository.GetUltimoEloAsync(jugador.IdUsuario);
        return [.. jugadores.OrderByDescending(j => elo[j.IdInscripcion]).ThenBy(_ => Random.Shared.Next())];
    }

    private static string Nick(InscripcionTorneo inscripcion) =>
        inscripcion.IdUsuarioNavigation?.Nick ?? $"inscripción {inscripcion.IdInscripcion}";

    private static bool TieneResultado(PartidaTorneoDTO partida) =>
        partida.ResultadoUsuario1 is not null
        || partida.ResultadoUsuario2 is not null
        || partida.PartidaValidadaUsuario1 == true
        || partida.PartidaValidadaUsuario2 == true;
}
