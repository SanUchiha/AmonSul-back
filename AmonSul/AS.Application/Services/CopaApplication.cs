using AS.Application.Exceptions;
using AS.Application.Interfaces;
using AS.Application.Utils;
using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;
using AS.Domain.Types.Torneo;
using AS.Infrastructure.Repositories.Interfaces;
using AS.Utils.Statics;

namespace AS.Application.Services;

/// <summary>
/// Torneos con formato Copa. Fase de grupos: repartir a los jugadores (por ELO con bombos, por sorteo o a mano) y
/// generar las jornadas de cada grupo, todos contra todos como en la liga. Eliminatoria: el cuadro con los clasificados,
/// que avanza solo al decidirse cada cruce, el tercer puesto y la clasificación final. Y retirar jugadores en las dos
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
            // En la eliminatoria lo jugado sigue contando, y en su cruce pendiente pasa el rival
            return await EnCuadroAsync(async () =>
            {
                await _unitOfWork.InscripcionRepository.ActualizarEstadoInscripcionAsync(
                    idInscripcion,
                    LigaApplication.ESTADO_RETIRADO
                );
                await SincronizarCuadroAsync(idTorneo);
            });

        await _unitOfWork.EnTransaccionAsync(async () =>
        {
            await _eloLigaApplication.QuitarDeJugadorAsync(idTorneo, inscripcion.IdUsuario);
            await _unitOfWork.InscripcionRepository.RetirarAsync(idInscripcion, LigaApplication.ESTADO_RETIRADO);
        });
        return (true, null);
    }

    // ---------- Eliminatoria ----------

    /// <summary>
    /// Cierra los grupos (lo que quede por jugar no cuenta) y monta el cuadro. Pasan los primeros de cada grupo que
    /// diga la copa; si no llenan un cuadro de 2, 4, 8..., entran los mejores siguientes (como los mejores terceros), y
    /// si ni así, los mejores pasan la primera ronda sin jugar. Siembra estándar: los dos mejores solo se pueden cruzar
    /// en la final y, si se puede, nadie empieza contra uno de su grupo
    /// </summary>
    public async Task<(bool Ok, string? Error)> GenerarEliminatoriaAsync(int idTorneo)
    {
        Torneo? copa = await GetCopaAsync(idTorneo);
        if (copa is null)
            return (false, "El torneo no es una copa.");
        if (await _unitOfWork.GanadorRepository.ExistsByTorneoAsync(idTorneo))
            return (false, "La copa ya está cerrada.");
        List<PartidaTorneo> partidas = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneo(idTorneo);
        if (!partidas.Any(p => p.Fase == FaseCopaType.GRUPOS))
            return (false, "Los grupos aún no tienen jornadas.");
        if (partidas.Any(EsDelCuadro))
            return (false, "La eliminatoria ya está generada.");

        List<ClasificacionJornadas.Fila> ranking = Ranking(copa, await JugadoresAsync(idTorneo), partidas);
        int directos = ranking.Count(f => f.Posicion <= copa.ClasificadosPorGrupo);
        if (directos < 2)
            return (false, "Hacen falta al menos 2 clasificados para la eliminatoria.");

        int tamano = PotenciaDeDos(directos);
        ClasificacionJornadas.Fila?[] huecos = Sembrar([.. ranking.Take(tamano)], tamano);
        Dictionary<int, int> posiciones = [];
        for (int i = 0; i < huecos.Length; i++)
            if (huecos[i] is { } fila)
                posiciones[fila.Inscripcion.IdInscripcion] = i + 1;

        return await EnCuadroAsync(async () =>
        {
            await _unitOfWork.InscripcionRepository.ActualizarPosicionesCuadroAsync(idTorneo, posiciones);
            await SincronizarCuadroAsync(idTorneo);
        });
    }

    /// <summary>
    /// Deshace el cuadro para volver a los grupos, solo si aún no hay ningún cruce con resultado
    /// </summary>
    public async Task<(bool Ok, string? Error)> EliminarEliminatoriaAsync(int idTorneo)
    {
        Torneo? copa = await GetCopaAsync(idTorneo);
        if (copa is null)
            return (false, "El torneo no es una copa.");
        if (await _unitOfWork.GanadorRepository.ExistsByTorneoAsync(idTorneo))
            return (false, "La copa ya está cerrada.");
        List<PartidaTorneo> cruces = [.. (await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneo(idTorneo)).Where(EsDelCuadro)];
        if (cruces.Count == 0)
            return (false, "La eliminatoria no está generada.");
        if (cruces.Any(TieneResultado))
            return (false, "Ya hay cruces con resultado: la eliminatoria no se puede eliminar.");

        await _unitOfWork.EnTransaccionAsync(async () =>
        {
            foreach (PartidaTorneo cruce in cruces)
                _unitOfWork.PartidaTorneoRepository.Quitar(cruce);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.InscripcionRepository.ActualizarPosicionesCuadroAsync(idTorneo, new Dictionary<int, int>());
        });
        return (true, null);
    }

    /// <summary>
    /// Si la copa deja los empates al organizador, él dice quién pasa en un cruce empatado y ya validado
    /// </summary>
    public async Task<(bool Ok, string? Error)> ElegirQuienPasaAsync(int idTorneo, int idPartida, int idUsuario)
    {
        Torneo? copa = await GetCopaAsync(idTorneo);
        if (copa is null)
            return (false, "El torneo no es una copa.");
        if (copa.DesempateEliminatoria != DesempateEliminatoriaType.ORGANIZADOR)
            return (false, "En esta copa los empates de la eliminatoria no los decide el organizador.");
        if (await _unitOfWork.GanadorRepository.ExistsByTorneoAsync(idTorneo))
            return (false, "La copa ya está cerrada.");

        PartidaTorneo? partida = (await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneo(idTorneo)).FirstOrDefault(p =>
            p.IdPartidaTorneo == idPartida && EsDelCuadro(p)
        );
        if (partida is null)
            return (false, "La partida no es de la eliminatoria de esta copa.");
        if (
            !ClasificacionJornadas.Jugada(partida)
            || partida.ResultadoUsuario1 is null
            || partida.ResultadoUsuario1 != partida.ResultadoUsuario2
        )
            return (false, "Solo se elige quién pasa en un empate ya validado por los dos.");
        if (idUsuario != partida.IdUsuario1 && idUsuario != partida.IdUsuario2)
            return (false, "Tiene que pasar uno de los dos jugadores del cruce.");

        return await EnCuadroAsync(async () =>
        {
            partida.IdUsuarioPasa = idUsuario;
            await _unitOfWork.SaveChangesAsync();
            await SincronizarCuadroAsync(idTorneo);
        });
    }

    /// <summary>
    /// Rehace el cuadro a partir de lo que hay: los huecos de la primera ronda y quién ha pasado cada cruce. Crea cada
    /// cruce en cuanto se conocen sus dos jugadores, cambia o quita los que aún no tienen resultado y no deja tocar los
    /// que ya lo tienen. Lo llama todo lo que puede cambiar quién pasa: generar, cada resultado, retirar y desempatar
    /// </summary>
    public async Task SincronizarCuadroAsync(int idTorneo)
    {
        Torneo? copa = await GetCopaAsync(idTorneo);
        if (copa is null)
            return;
        List<InscripcionTorneo> inscripciones = await _unitOfWork.InscripcionRepository.GetInscripcionesByTorneo(idTorneo);
        List<InscripcionTorneo> enCuadro = [.. inscripciones.Where(i => i.PosicionCuadro is not null)];
        if (enCuadro.Count == 0)
            return;

        List<PartidaTorneo> partidas = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneo(idTorneo);
        HashSet<int> retirados =
        [
            .. inscripciones.Where(i => i.EstadoInscripcion == LigaApplication.ESTADO_RETIRADO).Select(i => i.IdUsuario),
        ];
        Dictionary<int, int> puesto = Puestos(Ranking(copa, inscripciones, partidas));
        Dictionary<(string, int, int), PartidaTorneo> existentes = partidas
            .Where(EsDelCuadro)
            .ToDictionary(p => (p.Fase!, p.NumeroRonda ?? 0, p.Cruce ?? 0));
        HashSet<PartidaTorneo> vigentes = [];

        int tamano = PotenciaDeDos(enCuadro.Max(i => i.PosicionCuadro!.Value));
        int rondas = (int)Math.Log2(tamano);
        Lado[] nivel = [.. Enumerable.Repeat(Lado.Hueco, tamano)];
        foreach (InscripcionTorneo inscripcion in enCuadro)
            nivel[inscripcion.PosicionCuadro!.Value - 1] = new Lado(inscripcion, false);

        Lado[] perdedoresSemis = [Lado.Hueco, Lado.Hueco];
        for (int ronda = 1; ronda <= rondas; ronda++)
        {
            Lado[] siguiente = new Lado[nivel.Length / 2];
            for (int c = 0; c < siguiente.Length; c++)
            {
                (Lado pasa, Lado pierde) = Resolver(FaseCopaType.ELIMINATORIA, ronda, c + 1, nivel[2 * c], nivel[2 * c + 1]);
                siguiente[c] = pasa;
                if (ronda == rondas - 1)
                    perdedoresSemis[c] = pierde;
            }
            nivel = siguiente;
        }
        if (copa.TercerPuesto && rondas >= 2)
            Resolver(FaseCopaType.TERCER_PUESTO, rondas, 1, perdedoresSemis[0], perdedoresSemis[1]);

        // Lo que ya no corresponde a ningún cruce
        foreach (PartidaTorneo sobra in existentes.Values.Where(p => !vigentes.Contains(p)))
            Quitar(sobra);
        await _unitOfWork.SaveChangesAsync();

        (Lado Pasa, Lado Pierde) Resolver(string fase, int ronda, int cruce, Lado a, Lado b)
        {
            existentes.TryGetValue((fase, ronda, cruce), out PartidaTorneo? partida);
            if (a.Jugador is null || b.Jugador is null)
            {
                // Sin los dos no hay cruce: si el otro hueco es definitivo, pasa el que haya sin jugar
                if (a.Pendiente || b.Pendiente)
                    return (Lado.PorDecidir, Lado.PorDecidir);
                return (a.Jugador is not null ? a : b.Jugador is not null ? b : Lado.Hueco, Lado.Hueco);
            }

            if (partida is null)
            {
                partida = LigaApplication.NuevaPartida(copa, ronda, a.Jugador, b.Jugador);
                partida.Fase = fase;
                partida.Cruce = cruce;
                _unitOfWork.PartidaTorneoRepository.Agregar(partida);
            }
            else if (partida.IdUsuario1 != a.Jugador.IdUsuario || partida.IdUsuario2 != b.Jugador.IdUsuario)
            {
                if (TieneResultado(partida))
                    throw new ReglaTorneoException(
                        $"El cruce {cruce} de la ronda {ronda} ya tiene resultado con otros jugadores: corrígelo antes."
                    );
                partida.IdUsuario1 = a.Jugador.IdUsuario;
                partida.IdUsuario2 = b.Jugador.IdUsuario;
                partida.EjercitoUsuario1 = a.Jugador.EjercitoElegido;
                partida.EjercitoUsuario2 = b.Jugador.EjercitoElegido;
                partida.IdUsuarioPasa = null;
            }
            vigentes.Add(partida);

            partida.IdUsuarioPasa = Decidir(partida);
            if (partida.IdUsuarioPasa is null)
                return (Lado.PorDecidir, Lado.PorDecidir);
            return partida.IdUsuarioPasa == partida.IdUsuario1 ? (a, b) : (b, a);
        }

        int? Decidir(PartidaTorneo partida)
        {
            int jugador1 = partida.IdUsuario1;
            int jugador2 = partida.IdUsuario2;
            if (!ClasificacionJornadas.Jugada(partida) || partida.ResultadoUsuario1 is null || partida.ResultadoUsuario2 is null)
            {
                // Sin jugar, si uno se ha retirado pasa el otro
                bool fuera1 = retirados.Contains(jugador1);
                bool fuera2 = retirados.Contains(jugador2);
                return fuera1 == fuera2 ? null : fuera1 ? jugador2 : jugador1;
            }
            if (partida.ResultadoUsuario1 != partida.ResultadoUsuario2)
                return partida.ResultadoUsuario1 > partida.ResultadoUsuario2 ? jugador1 : jugador2;

            bool general1 = partida.LiderMuertoUsuario1 == true;
            bool general2 = partida.LiderMuertoUsuario2 == true;
            return copa.DesempateEliminatoria switch
            {
                DesempateEliminatoriaType.ORGANIZADOR =>
                    partida.IdUsuarioPasa == jugador1 || partida.IdUsuarioPasa == jugador2 ? partida.IdUsuarioPasa : null,
                DesempateEliminatoriaType.GENERAL when general1 != general2 => general1 ? jugador1 : jugador2,
                _ => puesto.GetValueOrDefault(jugador1, int.MaxValue) <= puesto.GetValueOrDefault(jugador2, int.MaxValue)
                    ? jugador1
                    : jugador2,
            };
        }

        void Quitar(PartidaTorneo partida)
        {
            if (TieneResultado(partida))
                throw new ReglaTorneoException(
                    $"El cruce {partida.Cruce} de la ronda {partida.NumeroRonda} ya tiene resultado y dejaría de jugarse: corrígelo antes."
                );
            _unitOfWork.PartidaTorneoRepository.Quitar(partida);
        }
    }

    /// <summary>
    /// Guarda la clasificación final: campeón y finalista, el tercer puesto (o los que pierden las semifinales), luego
    /// los que caen en cada ronda y por último los que no pasaron de grupos. Dentro de cada tramo, por cómo quedaron en
    /// grupos. Los retirados no cuentan
    /// </summary>
    public async Task<(bool Ok, string? Error)> CerrarAsync(int idTorneo)
    {
        Torneo? copa = await GetCopaAsync(idTorneo);
        if (copa is null)
            return (false, "El torneo no es una copa.");
        if (await _unitOfWork.GanadorRepository.ExistsByTorneoAsync(idTorneo))
            return (false, "La copa ya está cerrada.");

        List<InscripcionTorneo> inscripciones = await _unitOfWork.InscripcionRepository.GetInscripcionesByTorneo(idTorneo);
        List<InscripcionTorneo> enCuadro = [.. inscripciones.Where(i => i.PosicionCuadro is not null)];
        List<PartidaTorneo> partidas = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneo(idTorneo);
        if (enCuadro.Count == 0)
            return (false, "La eliminatoria aún no está generada.");

        int rondas = (int)Math.Log2(PotenciaDeDos(enCuadro.Max(i => i.PosicionCuadro!.Value)));
        List<PartidaTorneo> cruces = [.. partidas.Where(p => p.Fase == FaseCopaType.ELIMINATORIA)];
        PartidaTorneo? final = cruces.FirstOrDefault(p => p.NumeroRonda == rondas && p.Cruce == 1);
        if (final?.IdUsuarioPasa is null)
            return (false, "La final aún no está decidida.");
        PartidaTorneo? tercero = partidas.FirstOrDefault(p => p.Fase == FaseCopaType.TERCER_PUESTO);
        if (copa.TercerPuesto && rondas >= 2 && tercero?.IdUsuarioPasa is null)
            return (false, "El tercer puesto aún no está decidido.");

        HashSet<int> retirados =
        [
            .. inscripciones.Where(i => i.EstadoInscripcion == LigaApplication.ESTADO_RETIRADO).Select(i => i.IdUsuario),
        ];
        List<InscripcionTorneo> jugadores = await JugadoresAsync(idTorneo);
        Dictionary<int, int> puesto = Puestos(Ranking(copa, jugadores, partidas));
        List<int> orden = [];
        void Anadir(IEnumerable<int> ids)
        {
            foreach (int id in ids.OrderBy(id => puesto.GetValueOrDefault(id, int.MaxValue)))
                if (!orden.Contains(id) && !retirados.Contains(id))
                    orden.Add(id);
        }

        Anadir([final.IdUsuarioPasa.Value]);
        Anadir([Perdedor(final)]);
        if (tercero?.IdUsuarioPasa is not null && copa.TercerPuesto)
        {
            Anadir([tercero.IdUsuarioPasa.Value]);
            Anadir([Perdedor(tercero)]);
        }
        for (int ronda = rondas - 1; ronda >= 1; ronda--)
            Anadir(cruces.Where(p => p.NumeroRonda == ronda && p.IdUsuarioPasa is not null).Select(Perdedor));
        Anadir(jugadores.Select(j => j.IdUsuario));

        List<Ganador> ganadores =
        [
            .. orden.Select((idUsuario, i) => new Ganador { IdTorneo = idTorneo, IdUsuario = idUsuario, Resultado = i + 1 }),
        ];
        return await _unitOfWork.GanadorRepository.RegisterResultadosTorneoAsync(ganadores)
            ? (true, null)
            : (false, "La copa ya está cerrada.");
    }

    // Lo que toca el cuadro va todo junto y, si una regla lo impide, se deshace y se dice por qué
    private async Task<(bool Ok, string? Error)> EnCuadroAsync(Func<Task> accion)
    {
        try
        {
            await _unitOfWork.EnTransaccionAsync(accion);
            return (true, null);
        }
        catch (ReglaTorneoException ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Cada hueco de la primera ronda con su jugador, o vacío si el cuadro no se llena. Cada cruce es una cabeza de
    /// serie contra un rival; los rivales que existen se recolocan, lo menos posible, para que nadie empiece contra uno
    /// de su grupo. Si no hay forma, se queda la siembra estándar
    /// </summary>
    private static ClasificacionJornadas.Fila?[] Sembrar(List<ClasificacionJornadas.Fila> cuadro, int tamano)
    {
        // Orden estándar: el 1 contra el último y el 1 y el 2 en mitades distintas
        List<int> orden = [1, 2];
        while (orden.Count < tamano)
        {
            int total = orden.Count * 2 + 1;
            orden = [.. orden.SelectMany(s => new[] { s, total - s })];
        }
        ClasificacionJornadas.Fila? DeSemilla(int semilla) => semilla <= cuadro.Count ? cuadro[semilla - 1] : null;

        int cruces = tamano / 2;
        int[] rivales = [.. Enumerable.Range(0, cruces).Select(c => orden[2 * c + 1])];
        List<int> movibles = [.. Enumerable.Range(0, cruces).Where(c => rivales[c] <= cuadro.Count)];
        int[] solucion = (int[])rivales.Clone();
        HashSet<int> usados = [];
        int intentos = 0;
        bool Colocar(int k)
        {
            if (k == movibles.Count)
                return true;
            if (++intentos > 100_000)
                return false;
            int c = movibles[k];
            int? grupoCabeza = DeSemilla(orden[2 * c])!.Inscripcion.Grupo;
            foreach (int rival in movibles.Select(m => rivales[m]).Where(r => !usados.Contains(r)).OrderBy(r => Math.Abs(r - rivales[c])))
            {
                if (DeSemilla(rival)!.Inscripcion.Grupo == grupoCabeza)
                    continue;
                usados.Add(rival);
                solucion[c] = rival;
                if (Colocar(k + 1))
                    return true;
                usados.Remove(rival);
            }
            return false;
        }
        if (!Colocar(0))
            solucion = rivales;

        ClasificacionJornadas.Fila?[] huecos = new ClasificacionJornadas.Fila?[tamano];
        for (int c = 0; c < cruces; c++)
        {
            huecos[2 * c] = DeSemilla(orden[2 * c]);
            huecos[2 * c + 1] = DeSemilla(solucion[c]);
        }
        return huecos;
    }

    // Todos los que tienen grupo, ordenados entre grupos: primero los primeros de cada grupo, luego los segundos...
    private static List<ClasificacionJornadas.Fila> Ranking(
        Torneo copa,
        IEnumerable<InscripcionTorneo> jugadores,
        IEnumerable<PartidaTorneo> partidas
    )
    {
        List<PartidaTorneo> deGrupos = [.. partidas.Where(p => p.Fase == FaseCopaType.GRUPOS)];
        return
        [
            .. jugadores
                .Where(j => j.Grupo is not null)
                .GroupBy(j => j.Grupo)
                .SelectMany(g => ClasificacionJornadas.Clasificar(g, deGrupos.Where(p => p.Grupo == g.Key), copa.ClassificationType))
                .OrderBy(f => f.Posicion)
                .ThenBy(f => f, ClasificacionJornadas.Comparador),
        ];
    }

    private static Dictionary<int, int> Puestos(List<ClasificacionJornadas.Fila> ranking) =>
        ranking.Select((f, i) => (f.Inscripcion.IdUsuario, i)).ToDictionary(x => x.IdUsuario, x => x.i);

    private static int PotenciaDeDos(int n)
    {
        int potencia = 1;
        while (potencia < n)
            potencia *= 2;
        return potencia;
    }

    private static bool EsDelCuadro(PartidaTorneo partida) =>
        partida.Fase is FaseCopaType.ELIMINATORIA or FaseCopaType.TERCER_PUESTO;

    private static int Perdedor(PartidaTorneo partida) =>
        partida.IdUsuarioPasa == partida.IdUsuario1 ? partida.IdUsuario2 : partida.IdUsuario1;

    private static bool TieneResultado(PartidaTorneo partida) =>
        partida.ResultadoUsuario1 is not null
        || partida.ResultadoUsuario2 is not null
        || partida.PartidaValidadaUsuario1 == true
        || partida.PartidaValidadaUsuario2 == true;

    // Un lado de un cruce: un jugador, un hueco definitivo (nadie) o aún por decidir
    private readonly record struct Lado(InscripcionTorneo? Jugador, bool Pendiente)
    {
        public static readonly Lado Hueco = new(null, false);
        public static readonly Lado PorDecidir = new(null, true);
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
