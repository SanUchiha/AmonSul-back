using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IGanadorRepository
{
    Task<Ganador> GetById(int id);
    Task<List<Ganador>> GetAll();
    Task<List<Ganador>> GetAllByUsuario(int idUsuario);
    Task<bool> Register(Ganador usuario);
    Task<bool> RegisterResultadosTorneoAsync(List<Ganador> ganadores);
    Task<bool> Delete(int id);
    Task<bool> ExistsByTorneoAsync(int idTorneo);
    Task<int?> GetIdTorneoAsync(int idGanador);
    Task<List<Ganador>> GetByTorneoAsync(int idTorneo);
    Task<Dictionary<int, int>> GetTotalJugadoresAsync(IReadOnlyCollection<int> idsTorneo);
}
