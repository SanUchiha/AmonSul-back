using AS.Domain.Models;
using AS.Infrastructure.Data;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class LigaRepository(DbamonsulContext dbamonsulContext) : ILigaRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<bool> AddTorneoToLigaAsync(LigaTorneo ligaTorneo)
    {
        await _dbamonsulContext.LigaTorneo.AddAsync(ligaTorneo);
        await _dbamonsulContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteLigaTorneoAsync(int idLiga, int idTorneo)
    {
        LigaTorneo? ligaTorneo =
            await _dbamonsulContext.LigaTorneo
                .FirstOrDefaultAsync(lt => lt.IdLiga == idLiga && lt.IdTorneo == idTorneo);

        if (ligaTorneo == null)
            return false;

        _dbamonsulContext.LigaTorneo.Remove(ligaTorneo);
        await _dbamonsulContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<Liga>> GetAllLigasAsync() => 
        await _dbamonsulContext.Liga.ToListAsync();

    public async Task<Liga?> GetLigaByIdAsync(int idLiga) => 
        await _dbamonsulContext.Liga.FindAsync(idLiga);

    public async Task<List<Liga>?> GetLigasAsocidasATorneoAsync(int idTorneo) =>
        await _dbamonsulContext.Liga
            .Where(l => _dbamonsulContext.LigaTorneo
            .Any(lt => lt.IdTorneo == idTorneo && lt.IdLiga == l.IdLiga))
            .ToListAsync() ?? [];
    

    public async Task<List<Liga>?> GetLigasNoTorneoAsync(int idTorneo) =>
        await _dbamonsulContext.Liga
            .Where(l => !_dbamonsulContext.LigaTorneo
            .Any(lt => lt.IdTorneo == idTorneo && lt.IdLiga == l.IdLiga))
            .ToListAsync() ?? [];

    public async Task<List<LigaTorneo>?> GetTorneosByIdLigaAsync(int idLiga) =>
        await _dbamonsulContext.LigaTorneo
                .Include(lt => lt.Liga)
                .Include(lt => lt.Torneo)
                .Where(t => t.IdLiga == idLiga)
                .ToListAsync();
}

