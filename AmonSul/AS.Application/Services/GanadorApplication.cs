using AS.Application.DTOs.Ganador;
using AS.Application.Interfaces;
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

    public async Task<List<Ganador>> GetAll() => 
        await _unitOfWork.GanadorRepository.GetAll();

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
