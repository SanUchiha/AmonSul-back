using AS.Application.DTOs.CircuitoTorneo;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class CircuitoApplication(IUnitOfWork unitOfWork, IMapper mapper) : ICircuitoApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<bool> AddTorneoToCircuitoAsync(CircuitoTorneoDTO circuitoTorneoDTO) =>
        await _unitOfWork.CircuitoRepository.AddTorneoToCircuitoAsync(
            _mapper.Map<CircuitoTorneo>(circuitoTorneoDTO));

    public async Task<bool> DeleteCircuitoTorneoAsync(int idCircuito, int idTorneo) =>
        await _unitOfWork.CircuitoRepository.DeleteCircuitoTorneoAsync(idCircuito, idTorneo);

    public async Task<List<Circuito>> GetAllCircuitosAsync() =>
        await _unitOfWork.CircuitoRepository.GetAllCircuitosAsync();

    public async Task<Circuito?> GetCircuitoByIdAsync(int idCircuito) =>
        await _unitOfWork.CircuitoRepository.GetCircuitoByIdAsync(idCircuito);

    public async Task<List<Circuito>?> GetCircuitosAsociadosATorneoAsync(int idTorneo) =>
        await _unitOfWork.CircuitoRepository.GetCircuitosAsociadosATorneoAsync(idTorneo);
    

    public async Task<List<Circuito>?> GetCircuitosNoTorneoAsync(int idTorneo) =>
        await _unitOfWork.CircuitoRepository.GetCircuitosNoTorneoAsync(idTorneo);

    public async Task<List<CircuitoTorneo>?> GetTorneosByIdCircuitoAsync(int idCircuito) =>
        await _unitOfWork.CircuitoRepository.GetTorneosByIdCircuitoAsync(idCircuito);

    public async Task CrearCircuitoAmonSulAnualAsync()
    {
        int anio = DateTime.Now.Year;
        Circuito circuito = new() { NombreCircuito = $"Amon S\u00FBl {anio}" };
        await _unitOfWork.CircuitoRepository.CrearCircuitoAsync(circuito);
    }

    /// <summary>
    /// Un circuito nuevo desde la web: lo crea cualquiera y queda a su nombre. Null si ya hay uno que se llama así
    /// </summary>
    public async Task<Circuito?> CrearCircuitoAsync(string nombre, int idUsuario)
    {
        Circuito circuito = new() { NombreCircuito = nombre, IdUsuario = idUsuario };
        return await _unitOfWork.CircuitoRepository.CrearCircuitoAsync(circuito) ? circuito : null;
    }

    public async Task<bool> RenombrarCircuitoAsync(int idCircuito, string nombre) =>
        await _unitOfWork.CircuitoRepository.RenombrarCircuitoAsync(idCircuito, nombre);

    public async Task<bool> BorrarCircuitoAsync(int idCircuito) =>
        await _unitOfWork.CircuitoRepository.BorrarCircuitoAsync(idCircuito);
}
