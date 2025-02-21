using AS.Domain.Models;
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

    public async Task<List<Liga>> GetAllLigasAsync()
    {
        return await _dbamonsulContext.Liga.ToListAsync();
    }

    public async Task<Liga?> GetLigaByIdAsync(int idLiga) => 
        await _dbamonsulContext.Liga.FindAsync(idLiga);

    public async Task<List<LigaTorneo>?> GetTorneosByIdLigaAsync(int idLiga)
    { 
        return await _dbamonsulContext.LigaTorneo
                .Include(lt => lt.Liga)
                .Include(lt => lt.Torneo)
                .Where(t => t.IdLiga == idLiga)
                .ToListAsync();
    }
}

