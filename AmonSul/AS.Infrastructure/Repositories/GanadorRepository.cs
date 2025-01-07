using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
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

    public async Task<bool> ExistsByTorneoAsync(int idTorneo) => 
        await _dbamonsulContext.Ganador.AnyAsync(x => x.IdTorneo == idTorneo);

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
}