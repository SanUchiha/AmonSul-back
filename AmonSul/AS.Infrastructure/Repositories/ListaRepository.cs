using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class ListaRepository(DbamonsulContext dbamonsulContext) : IListaRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<Lista> Delete(int idLista)
    {
        var lista = await _dbamonsulContext.Listas.FindAsync(idLista);
        if (lista != null)
        {
            _dbamonsulContext.Listas.Remove(lista);
            await _dbamonsulContext.SaveChangesAsync();
        }
        return lista!;
    }

    public async Task<Lista> GetListaById(int idLista)
    {
        var lista = await _dbamonsulContext.Listas
            .Include(l => l.IdInscripcionNavigation)
            .FirstOrDefaultAsync(l => l.IdLista == idLista);
        if(lista == null) return null!;

        return lista;
    }

    public async Task<Lista> GetListaInscripcionById(int idInscripcion)
    {
        var lista = await _dbamonsulContext.Listas
            .Include(l => l.IdInscripcionNavigation)
            .FirstOrDefaultAsync(l => l.IdInscripcion == idInscripcion);

        if (lista == null) return null!;

        return lista;
    }

    public async Task<List<Lista>> GetListas()
    {
        var lista = await _dbamonsulContext.Listas
            .Include(l => l.IdInscripcionNavigation)
            .ToListAsync();

        if (lista == null) return null!;

        return lista;
    }

    public async Task<List<Lista>> GetListasByTorneo(int idTorneo)
    {
        var listas = await _dbamonsulContext.Listas
            .Include(l => l.IdInscripcionNavigation)
            .Where(l => l.IdInscripcionNavigation!.IdTorneo == idTorneo)
            .ToListAsync();

        if (listas == null) return null!;

        return listas;
    }

    public async Task<List<Lista>> GetListasByUser(int idUsuario)
    {
        return await _dbamonsulContext.Listas
            .Include(l => l.IdInscripcionNavigation)
            .Where(l => l.IdInscripcionNavigation!.IdUsuario == idUsuario)
            .ToListAsync();
    }

    public async Task<bool> RegisterLista(Lista lista)
    {
        await _dbamonsulContext.Listas.AddAsync(lista);
        await _dbamonsulContext.SaveChangesAsync();
        return true;
    }

    public async Task<Lista> UpdateLista(Lista lista)
    {
        var existingLista = await _dbamonsulContext.Listas.FindAsync(lista.IdLista);
        if (existingLista != null)
        {
            _dbamonsulContext.Entry(existingLista).CurrentValues.SetValues(lista);
            await _dbamonsulContext.SaveChangesAsync();
        }
        return existingLista!;
    }
}
