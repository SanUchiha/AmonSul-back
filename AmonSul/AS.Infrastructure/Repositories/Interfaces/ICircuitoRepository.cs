using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface ICircuitoRepository
{
    Task <List<Circuito>> GetAllCircuitosAsync();
    Task <Circuito?> GetCircuitoByIdAsync(int idCircuito);
    Task<List<CircuitoTorneo>?> GetTorneosByIdCircuitoAsync(int idCircuito);
    Task<bool> AddTorneoToCircuitoAsync(CircuitoTorneo circuitoTorneo);
    Task<List<Circuito>?> GetCircuitosNoTorneoAsync(int idTorneo);
    Task<List<Circuito>?> GetCircuitosAsociadosATorneoAsync(int idTorneo);
    Task<bool> DeleteCircuitoTorneoAsync(int idCircuito, int idTorneo);
    Task<Circuito?> GetCircuitoAmonSulUltimoAnioAsync();
    Task<bool> CrearCircuitoAsync(Circuito circuito);
}
