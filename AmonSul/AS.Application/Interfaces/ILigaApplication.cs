using AS.Application.DTOs.LigaTorneo;
using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface ILigaApplication
{
    Task<List<Liga>> GetAllLigasAsync();
    Task<Liga?> GetLigaByIdAsync(int idLiga);
    Task<List<LigaTorneo>?> GetTorneosByIdLigaAsync(int idLiga);
    Task<bool> AddTorneoToLigaAsync(LigaTorneoDTO ligaTorneoDTO);
}
