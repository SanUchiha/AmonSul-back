using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class InscripcionRepository(DbamonsulContext dbamonsulContext) : IInscripcionRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public Task<bool> CambiarEstadoInscripcion(InscripcionTorneo actualizarEstadoInscripcion)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CambiarEstadoLista(InscripcionTorneo actualizarEstadoLista)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CambiarEstadoPago(InscripcionTorneo actualizarEstadoPago)
    {
        throw new NotImplementedException();
    }

    public async Task<InscripcionTorneo> Delete(int idInscripcion)
    {
        var inscripcion = await _dbamonsulContext.InscripcionTorneos
                                                 .Include(i => i.Lista)
                                                 .FirstOrDefaultAsync(i => i.IdInscripcion == idInscripcion);
        if (inscripcion == null) return null!;

        _dbamonsulContext.InscripcionTorneos.Remove(inscripcion);
        await _dbamonsulContext.SaveChangesAsync();
        return inscripcion;
    }

    //Obtiene una ins por id
    public async Task<InscripcionTorneo> GetInscripcionById(int idInscripcion)
    {
        var insc = await _dbamonsulContext.InscripcionTorneos
            .Include(it => it.Lista)
            .FirstOrDefaultAsync(it => it.IdInscripcion == idInscripcion);

        return insc!;
    }

    //Obtiene una lista de todas las ins
    public async Task<List<InscripcionTorneo>> GetInscripciones()
    {
        return await _dbamonsulContext.InscripcionTorneos.ToListAsync();
    }

    //Obtiene todas las insc de un torneo
    public async Task<List<InscripcionTorneo>> GetInscripcionesByTorneo(int idTorneo)
    {
        return await _dbamonsulContext.InscripcionTorneos
            .Include(it => it.IdUsuarioNavigation)
            .Include(it => it.Lista)
            .Where(it => it.IdTorneo == idTorneo)
            .ToListAsync();
    }

    //Obtiene todas las ins de un usuario
    public async Task<List<InscripcionTorneo>> GetInscripcionesByUser(int idUsuario)
    {
        var insc = await _dbamonsulContext.InscripcionTorneos
                                      .Include(it => it.IdTorneoNavigation)
                                      .Include(it => it.IdUsuarioNavigation)
                                      .Where(it => it.IdUsuario == idUsuario)
                                      .ToListAsync();
        return insc;
    }

    //Registra una ins
    public async Task<bool> Register(InscripcionTorneo inscripcionTorneo)
    {
        try
        {
            _dbamonsulContext.InscripcionTorneos.Add(inscripcionTorneo);
            await _dbamonsulContext.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> Update(InscripcionTorneo inscripcionTorneo)
    {
        try
        {
            _dbamonsulContext.InscripcionTorneos.Update(inscripcionTorneo);
            await _dbamonsulContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Exception("La inscripción no se pudo actualizar porque fue modificado por otro usuario.");
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al actualizar la inscripción.", ex);
        }
    }
}
