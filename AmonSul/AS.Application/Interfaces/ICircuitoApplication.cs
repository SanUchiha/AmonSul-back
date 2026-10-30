using AS.Application.DTOs.CircuitoTorneo;
using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface ICircuitoApplication
{
    Task<List<Circuito>> GetAllCircuitosAsync();
    Task<Circuito?> GetCircuitoByIdAsync(int idCircuito);
    Task<List<CircuitoTorneo>?> GetTorneosByIdCircuitoAsync(int idCircuito);
    Task<bool> AddTorneoToCircuitoAsync(CircuitoTorneoDTO circuitoTorneoDTO);
    Task<List<Circuito>?> GetCircuitosNoTorneoAsync(int idTorneo);
    Task<List<Circuito>?> GetCircuitosAsociadosATorneoAsync(int idTorneo);
    Task<bool> DeleteCircuitoTorneoAsync(int idCircuito, int idTorneo);
    Task CrearCircuitoAmonSulAnualAsync();
    Task<Circuito?> CrearCircuitoAsync(string nombre, int idUsuario);
    Task<bool> RenombrarCircuitoAsync(int idCircuito, string nombre);
    Task<bool> BorrarCircuitoAsync(int idCircuito);
}
