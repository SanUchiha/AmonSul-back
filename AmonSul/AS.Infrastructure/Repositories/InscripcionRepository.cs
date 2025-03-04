using AS.Domain.DTOs.Equipo;
using AS.Domain.DTOs.Inscripcion;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AS.Infrastructure.Repositories;

public class InscripcionRepository(DbamonsulContext dbamonsulContext) : IInscripcionRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<bool> AddUsuarioToEquipoAsync(EquipoUsuario equipoUsuario)
    {
        _dbamonsulContext.EquipoUsuario.Add(equipoUsuario);
        return await _dbamonsulContext.SaveChangesAsync() > 0;
    }

    public Task<bool> CambiarEstadoInscripcion(InscripcionTorneo actualizarEstadoInscripcion)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CambiarEstadoLista(int idInscripcion, string estadoLista)
    {
        InscripcionTorneo? inscripcion = 
            await _dbamonsulContext.InscripcionTorneos
                .FirstOrDefaultAsync(i => i.IdInscripcion == idInscripcion);
        if(inscripcion == null) return false;

        inscripcion.EstadoLista = estadoLista;

        _dbamonsulContext.InscripcionTorneos.Update(inscripcion);
        await _dbamonsulContext.SaveChangesAsync();
        return true;
    }

    public Task<bool> CambiarEstadoPago(InscripcionTorneo actualizarEstadoPago)
    {
        throw new NotImplementedException();
    }

    public async Task<Equipo> CreateEquipoAsync(Equipo equipo)
    {
        _dbamonsulContext.Equipo.Add(equipo);
        await _dbamonsulContext.SaveChangesAsync();
        return equipo;
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

    public async Task<List<EquipoDTO>> GetAllEquiposByTorneoAsync(int idTorneo) =>
        await _dbamonsulContext.Equipo
            .Where(e => e.Inscripciones.Any(i => i.IdTorneo == idTorneo))
            .Select(e => new
            {
                e.IdEquipo,
                e.NombreEquipo,
                e.IdCapitan,
                Inscripciones = e.Inscripciones.Where(i => i.IdTorneo == idTorneo)
                .Select(i => new
                {
                    i.IdInscripcion,
                    i.IdTorneo,
                    i.IdUsuario,
                    i.FechaInscripcion,
                    i.EstadoLista,
                    i.FechaEntregaLista,
                    i.EsPago,
                    i.IdUsuarioNavigation!.Nick,
                    Lista = i.Lista.FirstOrDefault()
                }).ToList()
            })
            .ToListAsync()
            .ContinueWith(task => task.Result.Select(e => new EquipoDTO
            {
                IdEquipo = e.IdEquipo,
                NombreEquipo = e.NombreEquipo,
                IdCapitan = e.IdCapitan,
                Inscripciones = e.Inscripciones.Select(i => new InscripcionTorneoDTO
                {
                    IdInscripcion = i.IdInscripcion,
                    IdTorneo = i.IdTorneo,
                    IdUsuario = i.IdUsuario,
                    FechaInscripcion = i.FechaInscripcion,
                    EstadoLista = i.EstadoLista,
                    FechaEntregaLista = i.FechaEntregaLista,
                    EsPago = i.EsPago,
                    Nick = i.Nick,
                    IdLista = i.Lista != null ? i.Lista.IdLista : 0,
                    ListaData = i.Lista != null ? i.Lista.ListaData ?? null: null,
                    Ejercito = i.Lista != null ? i.Lista.Ejercito ?? null : null
                }).ToList()
            }).ToList());


    public async Task<List<InscripcionTorneo>> GetAllInscripcionesByEquipoAsync(int idEquipo)
    {
        var insc = await _dbamonsulContext.InscripcionTorneos
            .Include(it => it.Lista)
            .Where(it => it.IdEquipo == idEquipo)
            .ToListAsync();

        return insc!;
    }

    public async Task<Equipo?> GetEquipoByIdAsync(int id) => 
        await _dbamonsulContext.Equipo
            .Include(e => e.Miembros)
            .FirstOrDefaultAsync(e => e.IdEquipo == id);

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

    /// <summary>
    /// Obtiene todas las insc de un torneo
    /// </summary>
    /// <param name="idTorneo"></param>
    /// <returns></returns>
    public async Task<List<InscripcionTorneo>> GetInscripcionesByTorneo(int idTorneo)
    {
        return await _dbamonsulContext.InscripcionTorneos
            .Include(it => it.IdUsuarioNavigation) // Incluye el usuario relacionado
            .Include(it => it.Lista)              // Incluye la lista relacionada
            .Where(it => it.IdTorneo == idTorneo) // Filtra por el torneo
            .ToListAsync();
    }

    public async Task<List<InscripcionTorneo>> GetInscripcionesEquipoByUser(int idUsuario) => 
        await _dbamonsulContext.InscripcionTorneos
                .Include(it => it.IdTorneoNavigation)
                .Include(it => it.IdUsuarioNavigation)
                .Where(it => it.IdUsuario == idUsuario && it.IdEquipo != null)
                .ToListAsync();

    /// <summary>
    /// Obtiene todas las ins de un usuario
    /// </summary>
    /// <param name="idUsuario"></param>
    /// <returns></returns>
    public async Task<List<InscripcionTorneo>> GetInscripcionesIndividualByUser(int idUsuario) => 
        await _dbamonsulContext.InscripcionTorneos
                .Include(it => it.IdTorneoNavigation)
                .Include(it => it.IdUsuarioNavigation)
                .Where(it => it.IdUsuario == idUsuario && it.IdEquipo == null)
                .ToListAsync();

    //Registra una ins
    public async Task<bool> Register(InscripcionTorneo inscripcionTorneo)
    {
        try
        {
            _dbamonsulContext.InscripcionTorneos.Add(inscripcionTorneo);
            await _dbamonsulContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("Error al registrar la inscripción.", ex);
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
