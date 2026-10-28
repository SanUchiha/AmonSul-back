using AS.Application.DTOs.Elo;
using AS.Application.DTOs.Ganador;
using AS.Application.DTOs.PartidaTorneo;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Elos;
using AS.Domain.DTOs.Equipo;
using AS.Domain.Models;
using AS.Domain.Types.Torneo;
using AS.Infrastructure.Repositories.Interfaces;
using AS.Utils.Statics;
using AutoMapper;
using Hangfire;

namespace AS.Application.Services;

public class GanadorApplication(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IEloApplication eloApplication) : IGanadorApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IEloApplication _eloApplication = eloApplication;

    public async Task<bool> Delete(int id) =>
        await _unitOfWork.GanadorRepository.Delete(id);

    /// <summary>
    /// Obtiene todos los ganadores
    /// </summary>
    /// <returns></returns>
    public async Task<List<GanadorDTO>> GetAll()
    {
        List<Ganador> ganadores =
            await _unitOfWork.GanadorRepository.GetAll();

        if (ganadores is null) return [];

        // Conseguir nombre de los torneos.
        List<int> idsTorneos = [.. ganadores
            .Select(g => g.IdTorneo)
            .Distinct()];

        if (idsTorneos.Count <= 0) return [];

        var tournaments =
            await _unitOfWork.TorneoRepository.GetAllSoloNames();

        var nicks =
            await _unitOfWork.UsuarioRepository.GetAllSoloNicks();

        List<GanadorDTO> ganadoresMapper =
            _mapper.Map<List<GanadorDTO>>(ganadores);

        foreach (var item in ganadoresMapper)
        {
            item.NombreTorneo = tournaments.FirstOrDefault(t => t.IdTorneo == item.IdTorneo)!.TournamentName;
            item.Nick = nicks.FirstOrDefault(n => n.IdUsuario == item.IdUsuario)!.Nick;
        }

        return ganadoresMapper;
    }

    public async Task<Ganador> GetById(int id) =>
        await _unitOfWork.GanadorRepository.GetById(id);

    public async Task<bool> IsSave(int idTorneo) =>
        await _unitOfWork.GanadorRepository.ExistsByTorneoAsync(idTorneo);

    /// <summary>
    /// Solo el organizador puede guardar los resultados. Si el torneo no existe el organizador es 0 y no coincide
    /// </summary>
    public async Task<bool> PuedeGuardarResultadosAsync(int idTorneo, int idUsuarioSolicitante) =>
        await _unitOfWork.TorneoRepository.GetIdOrganizadorByIdTorneo(idTorneo) == idUsuarioSolicitante;

    public async Task<bool> Register(Ganador ganador) =>
        await _unitOfWork.GanadorRepository.Register(ganador);

    public async Task<bool> SaveResultTournamentAsync(GuardarResultadosDTO guardarResultadosDTO)
    {
        List<GanadorDTO> ganadores = guardarResultadosDTO.GanadoresDTO;
        int idTorneo = guardarResultadosDTO.GenerarRondaDTO.IdTorneo;
        Torneo? torneo = await _unitOfWork.TorneoRepository.GetTipoYModoListas(idTorneo);
        if (torneo?.TipoTorneo == TorneoType.PAREJAS)
            ganadores = await ConParejasCompletasAsync(ganadores, idTorneo);

        // Una fila por jugador: el índice único de torneo y jugador rechazaría el guardado entero
        ganadores = [.. ganadores.OrderBy(g => g.Resultado).DistinctBy(g => g.IdUsuario)];

        // False si otra petición lo guardó a la vez: sin esto el ELO del torneo se aplicaría dos veces
        if (!await _unitOfWork.GanadorRepository.RegisterResultadosTorneoAsync(
            _mapper.Map<List<Ganador>>(ganadores)))
            return false;

        string eloJobId =
            BackgroundJob.Enqueue(() =>
                ActualizarEloAsync(guardarResultadosDTO.GenerarRondaDTO));
        
        BackgroundJob.ContinueJobWith(
            eloJobId,
            () => _eloApplication.UpdateClasificacionEloCacheAsync());

        return true;
    }

    /// <summary>
    /// Parejas: los dos miembros quedan en la posición de su pareja, venga en la lista uno de ellos o los dos.
    /// Si vienen los dos con distinta posición, se queda la mejor
    /// </summary>
    private async Task<List<GanadorDTO>> ConParejasCompletasAsync(List<GanadorDTO> ganadores, int idTorneo)
    {
        List<EquipoDTO> parejas = await _unitOfWork.InscripcionRepository.GetAllEquiposByTorneoAsync(idTorneo);
        Dictionary<int, List<int>> miembrosPorJugador = [];
        foreach (EquipoDTO pareja in parejas)
        {
            List<int> miembros = [.. pareja.Inscripciones.Select(i => i.IdUsuario)];
            foreach (int idUsuario in miembros)
                miembrosPorJugador[idUsuario] = miembros;
        }

        List<GanadorDTO> resultado = [];
        HashSet<int> yaColocados = [];
        foreach (GanadorDTO ganador in ganadores.OrderBy(g => g.Resultado))
        {
            if (!miembrosPorJugador.TryGetValue(ganador.IdUsuario, out List<int>? miembrosPareja))
                miembrosPareja = [ganador.IdUsuario];

            foreach (int idUsuario in miembrosPareja)
            {
                if (!yaColocados.Add(idUsuario)) continue;

                resultado.Add(new GanadorDTO
                {
                    IdTorneo = ganador.IdTorneo,
                    IdUsuario = idUsuario,
                    Resultado = ganador.Resultado,
                });
            }
        }

        return resultado;
    }

    public async Task ActualizarEloAsync(GenerarRondaDTO request)
    {
        // Me traigo todas las partidas del torneo.
        List<PartidaTorneo> partidasTorneo = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneo(request.IdTorneo);

        // Ordeno por rondas
        partidasTorneo = [.. partidasTorneo.OrderBy(x => x.NumeroRonda)];

        Torneo? torneo = await _unitOfWork.TorneoRepository.GetTipoYModoListas(request.IdTorneo);
        if (torneo?.TipoTorneo == TorneoType.PAREJAS)
        {
            await ActualizarEloParejasAsync(partidasTorneo);
            return;
        }

        // si para la ronda 1, no hay mas de 7 partidas. No cuenta para el elo
        if (partidasTorneo.Where(x => x.NumeroRonda == 1).Count() <= 7) return;

        // Actualizamos el elo para todos los jugadores
        foreach (PartidaTorneo partida in partidasTorneo)
        {
            int eloJugador1 = await _eloApplication.GetLastElo((int)partida.IdUsuario1!);
            int eloJugador2 = await _eloApplication.GetLastElo((int)partida.IdUsuario2!);

            double scoreGanador = 1.0;
            double scorePerdedor = 0.0;
            double scoreEmpate = 0.5;
            int nuevoEloJugador1 = 800;
            int nuevoEloJugador2 = 800;

            if (partida.GanadorPartidaTorneo == partida.IdUsuario1)
            {
                nuevoEloJugador1 = EloRating.CalculateNewRating(eloJugador1, eloJugador2, scoreGanador);
                nuevoEloJugador2 = EloRating.CalculateNewRating(eloJugador2, eloJugador1, scorePerdedor);
            }
            if (partida.GanadorPartidaTorneo == partida.IdUsuario2)
            {
                nuevoEloJugador1 = EloRating.CalculateNewRating(eloJugador1, eloJugador2, scorePerdedor);
                nuevoEloJugador2 = EloRating.CalculateNewRating(eloJugador2, eloJugador1, scoreGanador);
            }
            if (partida.GanadorPartidaTorneo == null)
            {
                nuevoEloJugador1 = EloRating.CalculateNewRating(eloJugador1, eloJugador2, scoreEmpate);
                nuevoEloJugador2 = EloRating.CalculateNewRating(eloJugador2, eloJugador1, scoreEmpate);
            }

            CreateEloDTO createElo1 = new()
            {
                IdUsuario = (int)partida.IdUsuario1,
                PuntuacionElo = nuevoEloJugador1,
                IdPartidaTorneo = partida.IdPartidaTorneo,
            };
            if (createElo1.IdUsuario != 568)
                await _eloApplication.RegisterElo(createElo1);

            CreateEloDTO createElo2 = new()
            {
                IdUsuario = (int)partida.IdUsuario2,
                PuntuacionElo = nuevoEloJugador2,
                IdPartidaTorneo = partida.IdPartidaTorneo,
            };
            if (createElo2.IdUsuario != 568)
                await _eloApplication.RegisterElo(createElo2);
        }
    }

    /// <summary>
    /// Parejas: el ELO de cada pareja es la media de sus jugadores y la variación de pareja contra
    /// pareja se suma o resta igual a los dos. Cuenta con 16 jugadores o más en la ronda 1, sin el bye
    /// </summary>
    private async Task ActualizarEloParejasAsync(List<PartidaTorneo> partidasTorneo)
    {
        int jugadoresRonda1 = partidasTorneo
            .Where(p => p.NumeroRonda == 1)
            .SelectMany(p => new[] { p.IdUsuario1, p.IdUsuario2, p.IdCompanero1, p.IdCompanero2 })
            .Where(id => id is not null && id != 568)
            .Distinct()
            .Count();
        if (jugadoresRonda1 < 16) return;

        foreach (PartidaTorneo partida in partidasTorneo)
        {
            List<int> pareja1 = [partida.IdUsuario1];
            if (partida.IdCompanero1 is int idCompanero1) pareja1.Add(idCompanero1);
            List<int> pareja2 = [partida.IdUsuario2];
            if (partida.IdCompanero2 is int idCompanero2) pareja2.Add(idCompanero2);

            Dictionary<int, int> eloActual = [];
            foreach (int idUsuario in pareja1.Concat(pareja2))
                eloActual[idUsuario] = await _eloApplication.GetLastElo(idUsuario);

            int eloPareja1 = (int)Math.Round(pareja1.Average(id => eloActual[id]));
            int eloPareja2 = (int)Math.Round(pareja2.Average(id => eloActual[id]));

            // Sin ganador cuenta como empate, igual que en individual
            double scorePareja1 = 0.5;
            if (partida.GanadorPartidaTorneo == partida.IdUsuario1) scorePareja1 = 1.0;
            if (partida.GanadorPartidaTorneo == partida.IdUsuario2) scorePareja1 = 0.0;

            int variacionPareja1 =
                EloRating.CalculateNewRating(eloPareja1, eloPareja2, scorePareja1) - eloPareja1;
            int variacionPareja2 =
                EloRating.CalculateNewRating(eloPareja2, eloPareja1, 1.0 - scorePareja1) - eloPareja2;

            foreach (int idUsuario in pareja1.Where(id => id != 568))
                await _eloApplication.RegisterElo(new CreateEloDTO
                {
                    IdUsuario = idUsuario,
                    PuntuacionElo = eloActual[idUsuario] + variacionPareja1,
                    IdPartidaTorneo = partida.IdPartidaTorneo,
                });

            foreach (int idUsuario in pareja2.Where(id => id != 568))
                await _eloApplication.RegisterElo(new CreateEloDTO
                {
                    IdUsuario = idUsuario,
                    PuntuacionElo = eloActual[idUsuario] + variacionPareja2,
                    IdPartidaTorneo = partida.IdPartidaTorneo,
                });
        }
    }
}