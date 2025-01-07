using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface ILigaRepository
{
    Task <List<Liga>> GetAllLigasAsync();
    Task <Liga?> GetLigaByIdAsync(int idLiga);
    Task<List<LigaTorneo>?> GetTorneosByIdLigaAsync(int idLiga);
    Task <bool> AddTorneoToLigaAsync();
}
