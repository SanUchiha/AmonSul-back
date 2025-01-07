using AS.Application.DTOs.Ganador;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class GanadorApplication(
    IUnitOfWork unitOfWork,
    IMapper mapper) : IGanadorApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

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
        List<int> idsTorneos = ganadores
            .Select(g => g.IdTorneo)
            .Distinct()
            .ToList();

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

    public async Task<bool> Register(List<GanadorDTO> ganadoresDTO)
    {
        foreach (GanadorDTO item in ganadoresDTO)
        {
            Ganador ganador = _mapper.Map<Ganador>(item);
            await _unitOfWork.GanadorRepository.Register(ganador);
        }

        return true;
    }
}
