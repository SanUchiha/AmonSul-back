using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class LigaRepository(DbamonsulContext dbamonsulContext) : ILigaRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public Task<bool> AddTorneoToLigaAsync()
    {
        throw new NotImplementedException();
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

