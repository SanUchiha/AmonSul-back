using AS.Domain.DTOs.Circuitos;
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

    // Para la lista: los años en que empiezan sus eventos y cuántos son. En dos consultas que se juntan aquí:
    // EF no sabe traducir la lista de años dentro de la de circuitos
    public async Task<List<CircuitoListadoDTO>> GetCircuitosParaListadoAsync()
    {
        var circuitos = await _dbamonsulContext.Circuito
            .AsNoTracking()
            .Select(c => new { c.IdCircuito, c.NombreCircuito, c.IdUsuario })
            .ToListAsync();
        var eventos = await _dbamonsulContext.CircuitoTorneo
            .AsNoTracking()
            .Select(ct => new { ct.IdCircuito, Anio = ct.Torneo!.FechaInicioTorneo.Year })
            .ToListAsync();
        var porCircuito = eventos.GroupBy(e => e.IdCircuito).ToDictionary(g => g.Key, g => g.Select(e => e.Anio).ToList());

        return [.. circuitos.Select(c =>
        {
            List<int> anios = porCircuito.GetValueOrDefault(c.IdCircuito) ?? [];
            return new CircuitoListadoDTO
            {
                IdCircuito = c.IdCircuito,
                NombreCircuito = c.NombreCircuito,
                IdUsuario = c.IdUsuario,
                Anios = [.. anios.Distinct().Order()],
                NumeroEventos = anios.Count,
            };
        })];
    }

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

    // La columna no distingue mayúsculas ni tildes
    public async Task<Circuito?> GetCircuitoByNombreAsync(string nombre) =>
        await _dbamonsulContext.Circuito.FirstOrDefaultAsync(l => l.NombreCircuito == nombre);

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

