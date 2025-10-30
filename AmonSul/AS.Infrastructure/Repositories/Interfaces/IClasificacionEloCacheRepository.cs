using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IClasificacionEloCacheRepository
{
    Task<bool> ClearCacheAsync();
    Task<bool> InsertCacheBatchAsync(List<ClasificacionEloCache> clasificaciones);
    Task<List<ClasificacionEloCache>> GetClasificacionCacheAsync();
}