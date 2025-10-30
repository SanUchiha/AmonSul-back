using AS.Application.DTOs.Elo;
using AS.Application.DTOs.Ganador;
using AS.Application.DTOs.PartidaTorneo;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Elos;
using AS.Domain.Models;
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

    public async Task<bool> Register(Ganador ganador) =>
        await _unitOfWork.GanadorRepository.Register(ganador);

    public async Task<bool> SaveResultTournamentAsync(GuardarResultadosDTO guardarResultadosDTO)
    {
        foreach (GanadorDTO item in guardarResultadosDTO.GanadoresDTO)
        {
            Ganador ganador = _mapper.Map<Ganador>(item);
            await _unitOfWork.GanadorRepository.Register(ganador);
        }

        string eloJobId =
            BackgroundJob.Enqueue(() =>
                ActualizarEloAsync(guardarResultadosDTO.GenerarRondaDTO));
        
        BackgroundJob.ContinueJobWith(
            eloJobId,
            () => _eloApplication.UpdateClasificacionEloCacheAsync());

        return true;
    }

    public async Task ActualizarEloAsync(GenerarRondaDTO request)
    {
        // Me traigo todas las partidas del torneo.
        List<PartidaTorneo> partidasTorneo = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneo(request.IdTorneo);

        // Ordeno por rondas
        partidasTorneo = [.. partidasTorneo.OrderBy(x => x.NumeroRonda)];

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
                PuntuacionElo = nuevoEloJugador1
            };
            if (createElo1.IdUsuario != 568)
                await _eloApplication.RegisterElo(createElo1);

            CreateEloDTO createElo2 = new()
            {
                IdUsuario = (int)partida.IdUsuario2,
                PuntuacionElo = nuevoEloJugador2
            };
            if (createElo2.IdUsuario != 568)
                await _eloApplication.RegisterElo(createElo2);
        }
    }
}