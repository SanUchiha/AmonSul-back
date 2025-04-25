using AS.Domain.DTOs.Lista;
using AS.Domain.Models;
using AS.Infrastructure.DTOs.Lista;
using AS.Infrastructure.Repositories.Interfaces;
using Azure.Core;
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
        Lista? lista = await _dbamonsulContext.Listas
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

    public async Task<List<Lista>> GetListasByInscripcion(int idInscripcion)
    {
        List<Lista> result = await _dbamonsulContext.Listas
            .Where(l => l.IdInscripcion.Equals(idInscripcion)).ToListAsync();

        if (result.Count<=0) return [];

        return result;
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

    public async Task<Lista> GetListaTorneo(int idTorneo, int idUsuario)
    {
        List<Lista> listas = await _dbamonsulContext.InscripcionTorneos
                                            .Where(it => it.IdTorneo == idTorneo && it.IdUsuario == idUsuario)
                                            .SelectMany(it => it.Lista)
                                            .ToListAsync();

        if (listas == null || listas.Count == 0) return null!;

        Lista lista = listas[0];

        return lista;
    }

    public async Task<ResultRegisterListarDTO> RegisterLista(Lista lista)
    {
        try
        {
            lista.EstadoLista = "ENTREGADA";
            await _dbamonsulContext.Listas.AddAsync(lista);
            await _dbamonsulContext.SaveChangesAsync();
            return new ResultRegisterListarDTO 
            {
                Result = true,
                IdLista = lista.IdLista,
            };
        }
        catch (Exception ex) 
        {
            return new ResultRegisterListarDTO
            {
                Result = false,
                Mensaje = ex.Message
            };
        }
    }

    public async Task<bool> UpdateEstadoLista(UpdateEstadoListaDTO request)
    {
        Lista? existingLista = await _dbamonsulContext.Listas.FindAsync(request.IdLista);

        if (existingLista != null)
        {
            existingLista.EstadoLista = request.Estado.ToUpper().Trim();
            _dbamonsulContext.Entry(existingLista).CurrentValues.SetValues(existingLista);
            await _dbamonsulContext.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<Lista> UpdateLista(UpdateListaDTO updateListaDTO)
    {
        updateListaDTO.FechaEntrega = DateOnly.FromDateTime(DateTime.Now);
        Lista? existingLista = await _dbamonsulContext.Listas.FindAsync(updateListaDTO.IdLista);

        if (existingLista != null)
        {
            existingLista.FechaEntrega = updateListaDTO.FechaEntrega;
            existingLista.ListaData = updateListaDTO.ListaData;
            existingLista.Bando = updateListaDTO.Ejercito.Band;
            existingLista.Ejercito = updateListaDTO.Ejercito.Name;
            existingLista.EstadoLista = "ENTREGADA";
            _dbamonsulContext.Entry(existingLista).CurrentValues.SetValues(existingLista);
            await _dbamonsulContext.SaveChangesAsync();
        }
        return existingLista!;
    }
}
