using AS.Domain.Models;
using AS.Infrastructure.Data;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class GanadorRepository(DbamonsulContext dbamonsulContext) : IGanadorRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<bool> Delete(int id)
    {
        try
        {
            var ganador = await _dbamonsulContext.Ganador.FindAsync(id);
            if (ganador == null) return false;
            
            _dbamonsulContext.Ganador.Remove(ganador);
            await _dbamonsulContext.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al eliminar el ganador.", ex);
        }
    }

    public async Task<List<Ganador>> GetAll()
    {
        try
        {
            List<Ganador> ganadores = await _dbamonsulContext.Ganador
                .ToListAsync();

            return ganadores;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener los ganadores.", ex);
        }
    }

    public async Task<List<Ganador>> GetAllByUsuario(int idUsuario)
    {
        try
        {
            return await _dbamonsulContext.Ganador
                .Where(g => g.IdUsuario == idUsuario)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener los ganadores.", ex);
        }
    }

    public async Task<int?> GetIdTorneoAsync(int idGanador) =>
        await _dbamonsulContext.Ganador
            .Where(g => g.IdGanador == idGanador)
            .Select(g => (int?)g.IdTorneo)
            .FirstOrDefaultAsync();

    public async Task<bool> ExistsByTorneoAsync(int idTorneo) => 
        await _dbamonsulContext.Ganador.AnyAsync(x => x.IdTorneo == idTorneo);

    public async Task<List<Ganador>> GetByTorneoAsync(int idTorneo) =>
        await _dbamonsulContext.Ganador
            .Where(g => g.IdTorneo == idTorneo)
            .OrderBy(g => g.Resultado)
            .ToListAsync();

    /// <summary>
    /// Cuántos jugadores tiene la clasificación final de cada torneo
    /// </summary>
    public async Task<Dictionary<int, int>> GetTotalJugadoresAsync(IReadOnlyCollection<int> idsTorneo) =>
        await _dbamonsulContext.Ganador
            .Where(g => idsTorneo.Contains(g.IdTorneo))
            .GroupBy(g => g.IdTorneo)
            .Select(g => new { IdTorneo = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.IdTorneo, x => x.Total);

    public async Task<Ganador> GetById(int id)
    {
        try
        {
            Ganador? ganador = await _dbamonsulContext.Ganador
                .FirstOrDefaultAsync(g => g.IdGanador == id);

            return ganador ??
                throw new KeyNotFoundException($"No se encontró un ganador con ID {id}.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Ocurrió un problema al obtener el ganador con ID {id}.", ex);
        }
    }

    public async Task<bool> Register(Ganador ganador)
    {
        try
        {
            await _dbamonsulContext.Ganador.AddAsync(ganador);
            await _dbamonsulContext.SaveChangesAsync();
            return true; 
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al registrar el ganador.", ex);
        }
    }

    /// <summary>
    /// Todas las posiciones del torneo en un solo SaveChanges: o se guardan todas o ninguna.
    /// False si ya estaban guardadas, por el índice único de torneo y jugador
    /// </summary>
    public async Task<bool> RegisterResultadosTorneoAsync(List<Ganador> ganadores)
    {
        try
        {
            await _dbamonsulContext.Ganador.AddRangeAsync(ganadores);
            await _dbamonsulContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // Otra petición guardó el torneo a la vez: se deja como estaba
            _dbamonsulContext.ChangeTracker.Clear();
            return false;
        }
    }
}