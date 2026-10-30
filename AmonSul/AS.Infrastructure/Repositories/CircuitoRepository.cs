using AS.Domain.Models;
using AS.Infrastructure.Data;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class CircuitoRepository(DbamonsulContext dbamonsulContext) : ICircuitoRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<bool> AddTorneoToCircuitoAsync(CircuitoTorneo circuitoTorneo)
    {
        await _dbamonsulContext.CircuitoTorneo.AddAsync(circuitoTorneo);
        await _dbamonsulContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteCircuitoTorneoAsync(int idCircuito, int idTorneo)
    {
        CircuitoTorneo? circuitoTorneo =
            await _dbamonsulContext.CircuitoTorneo
                .FirstOrDefaultAsync(lt => lt.IdCircuito == idCircuito && lt.IdTorneo == idTorneo);

        if (circuitoTorneo == null)
            return false;

        _dbamonsulContext.CircuitoTorneo.Remove(circuitoTorneo);
        await _dbamonsulContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<Circuito>> GetAllCircuitosAsync() => 
        await _dbamonsulContext.Circuito.ToListAsync();

    public async Task<Circuito?> GetCircuitoByIdAsync(int idCircuito) => 
        await _dbamonsulContext.Circuito.FindAsync(idCircuito);

    public async Task<List<Circuito>?> GetCircuitosAsociadosATorneoAsync(int idTorneo) =>
        await _dbamonsulContext.Circuito
            .Where(l => _dbamonsulContext.CircuitoTorneo
            .Any(lt => lt.IdTorneo == idTorneo && lt.IdCircuito == l.IdCircuito))
            .ToListAsync() ?? [];
    

    public async Task<List<Circuito>?> GetCircuitosNoTorneoAsync(int idTorneo) =>
        await _dbamonsulContext.Circuito
            .Where(l => !_dbamonsulContext.CircuitoTorneo
            .Any(lt => lt.IdTorneo == idTorneo && lt.IdCircuito == l.IdCircuito))
            .ToListAsync() ?? [];

    public async Task<List<CircuitoTorneo>?> GetTorneosByIdCircuitoAsync(int idCircuito) =>
        await _dbamonsulContext.CircuitoTorneo
                .Include(lt => lt.Circuito)
                .Include(lt => lt.Torneo)
                .Where(t => t.IdCircuito == idCircuito)
                .ToListAsync();

    // Solo los de la web, sin creador: uno de un usuario que empezara igual se llevaría todos los torneos nuevos
    public async Task<Circuito?> GetCircuitoAmonSulUltimoAnioAsync() =>
        await _dbamonsulContext.Circuito
            .Where(l => l.IdUsuario == null && l.NombreCircuito.StartsWith("Amon S"))
            .OrderByDescending(l => l.NombreCircuito)
            .FirstOrDefaultAsync();

    public async Task<bool> CrearCircuitoAsync(Circuito circuito)
    {
        bool yaExiste = await _dbamonsulContext.Circuito
            .AnyAsync(l => l.NombreCircuito == circuito.NombreCircuito);

        if (yaExiste)
            return false;

        await _dbamonsulContext.Circuito.AddAsync(circuito);
        await _dbamonsulContext.SaveChangesAsync();
        return true;
    }

    // False si otro circuito ya se llama así. La columna no distingue mayúsculas ni tildes
    public async Task<bool> RenombrarCircuitoAsync(int idCircuito, string nombre)
    {
        bool yaExiste = await _dbamonsulContext.Circuito
            .AnyAsync(l => l.IdCircuito != idCircuito && l.NombreCircuito == nombre);
        Circuito? circuito = await _dbamonsulContext.Circuito.FindAsync(idCircuito);
        if (yaExiste || circuito is null)
            return false;

        circuito.NombreCircuito = nombre;
        await _dbamonsulContext.SaveChangesAsync();
        return true;
    }

    // Los torneos no se tocan: solo dejan de estar en él
    public async Task<bool> BorrarCircuitoAsync(int idCircuito)
    {
        Circuito? circuito = await _dbamonsulContext.Circuito.FindAsync(idCircuito);
        if (circuito is null)
            return false;

        _dbamonsulContext.CircuitoTorneo.RemoveRange(
            _dbamonsulContext.CircuitoTorneo.Where(lt => lt.IdCircuito == idCircuito));
        _dbamonsulContext.Circuito.Remove(circuito);
        await _dbamonsulContext.SaveChangesAsync();
        return true;
    }
}

