using AS.Application.DTOs.CircuitoTorneo;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AS.Utils.Constantes;
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

    public async Task CrearCircuitoAmonSulAnualAsync() => await GetOrCrearAmonSulAsync(DateTime.Now.Year);

    /// <summary>
    /// El Amon Sûl de ese año; si aún no existe, lo crea a nombre de la administración
    /// </summary>
    private async Task<Circuito> GetOrCrearAmonSulAsync(int anio)
    {
        string nombre = ConstCircuitoAmonSul.Nombre(anio);
        Circuito? circuito = await _unitOfWork.CircuitoRepository.GetCircuitoByNombreAsync(nombre);
        if (circuito is not null)
            return circuito;

        // Si otra petición lo crea a la vez, este no se guarda y se usa el suyo
        await _unitOfWork.CircuitoRepository.CrearCircuitoAsync(
            new Circuito { NombreCircuito = nombre, IdUsuario = ConstCircuitoAmonSul.IdUsuarioCreador });
        return (await _unitOfWork.CircuitoRepository.GetCircuitoByNombreAsync(nombre))!;
    }

    /// <summary>
    /// Todo evento suma en el Amon Sûl del año en que empieza, y solo en ese: al crearlo y cada vez que se modifica,
    /// por si cambia de fecha
    /// </summary>
    public async Task AsociarAmonSulAsync(int idTorneo, DateOnly fechaInicio)
    {
        Circuito amonSul = await GetOrCrearAmonSulAsync(fechaInicio.Year);
        List<Circuito> asociados =
            await _unitOfWork.CircuitoRepository.GetCircuitosAsociadosATorneoAsync(idTorneo) ?? [];

        foreach (Circuito otro in asociados.Where(c =>
                     c.IdCircuito != amonSul.IdCircuito && ConstCircuitoAmonSul.EsAmonSul(c.NombreCircuito)))
            await _unitOfWork.CircuitoRepository.DeleteCircuitoTorneoAsync(otro.IdCircuito, idTorneo);

        if (!asociados.Any(c => c.IdCircuito == amonSul.IdCircuito))
            await _unitOfWork.CircuitoRepository.AddTorneoToCircuitoAsync(
                new CircuitoTorneo { IdCircuito = amonSul.IdCircuito, IdTorneo = idTorneo });
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
