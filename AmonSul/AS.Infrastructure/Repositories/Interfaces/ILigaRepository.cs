using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface ILigaRepository
{
    Task <List<Liga>> GetAllLigasAsync();
    Task <Liga?> GetLigaByIdAsync(int idLiga);
    Task<List<LigaTorneo>?> GetTorneosByIdLigaAsync(int idLiga);
    Task<bool> AddTorneoToLigaAsync(LigaTorneo ligaTorneo);
    Task<List<Liga>?> GetLigasNoTorneoAsync(int idTorneo);
    Task<List<Liga>?> GetLigasAsocidasATorneoAsync(int idTorneo);
    Task<bool> DeleteLigaTorneoAsync(int idLiga, int idTorneo);
}
