using AS.Domain.Models;
using AS.Infrastructure.Data;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class ClasificacionEloCacheRepository(DbamonsulContext dbamonsulContext) : IClasificacionEloCacheRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<bool> ClearCacheAsync()
    {
        try
        {
            // Eliminar todos los registros de la tabla caché
            await _dbamonsulContext.Database.ExecuteSqlRawAsync("DELETE FROM Clasificacion_Elo_Cache");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> InsertCacheBatchAsync(List<ClasificacionEloCache> clasificaciones)
    {
        try
        {
            await _dbamonsulContext.ClasificacionEloCache.AddRangeAsync(clasificaciones);
            await _dbamonsulContext.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<ClasificacionEloCache>> GetClasificacionCacheAsync()
    {
        return await _dbamonsulContext.ClasificacionEloCache
            .OrderByDescending(c => c.Elo)
            .ToListAsync();
    }
}