﻿using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class PartidaTorneoRepository(DbamonsulContext dbamonsulContext) : IPartidaTorneoRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<bool> Delete(int idPartida)
    {
        try
        {
            PartidaTorneo? partida = await _dbamonsulContext.PartidaTorneos.FindAsync(idPartida);
            if (partida == null) return false;

            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PartidaTorneo>? existingEntity = _dbamonsulContext.ChangeTracker
                                    .Entries<PartidaTorneo>()
                                    .FirstOrDefault(e => e.Entity.IdPartidaTorneo == partida.IdPartidaTorneo);

            if (existingEntity != null)
            {
                _dbamonsulContext.Entry(existingEntity.Entity).State = EntityState.Detached;
            }

            _dbamonsulContext.PartidaTorneos.Remove(partida);
            await _dbamonsulContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema en el servidor al eliminar la partida.", ex);
        }
    }

    public async Task<List<PartidaTorneo>> GetPartidasTorneos()
    {
        try
        {
            List<PartidaTorneo> response = await _dbamonsulContext.PartidaTorneos.ToListAsync();
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor.", ex);
        }
    }

    public async Task<PartidaTorneo> GetById(int idPartida)
    {
        try
        {
            PartidaTorneo? response = await _dbamonsulContext.PartidaTorneos
                .AsNoTracking()
                .Where(x => x.IdPartidaTorneo == idPartida)
                .FirstOrDefaultAsync();
            return response!;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor.", ex);
        }
    }

    public async Task<List<PartidaTorneo>> GetPartidasTorneo(int idTorneo)
    {
        try
        {
            List<PartidaTorneo> partidas = await _dbamonsulContext.PartidaTorneos
                .Where(p => p.IdTorneo == idTorneo)
                .ToListAsync();

            if (partidas == null) return [];
            return partidas;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al buscar las partidas", ex);
        }
    }

    public async Task<List<PartidaTorneo>> GetPartidasTorneoByRonda(int idTorneo, int ronda)
    {
        try
        {
            List<PartidaTorneo> partidas = await _dbamonsulContext.PartidaTorneos
                .Where(p => p.IdTorneo == idTorneo && 
                            p.NumeroRonda == ronda)
                .ToListAsync();

            if (partidas == null) return [];
            return partidas;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al buscar las partidas", ex);
        }
    }

    public async Task<List<PartidaTorneo>> GetPartidasTorneosByUsuario(int idUsuario)
    {
        try
        {
            List<PartidaTorneo> partidas = await _dbamonsulContext.PartidaTorneos
                .Where(p => p.IdUsuario1 == idUsuario || 
                            p.IdUsuario2 == idUsuario)
                .ToListAsync();

            if (partidas == null) return [];
            return partidas;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al buscar las partidas", ex);
        }
    }

    public async Task<List<PartidaTorneo>> GetPartidasTorneoByUsuario(int idTorneo, int idUsuario)
    {
        try
        {
            List<PartidaTorneo> partidas = await _dbamonsulContext.PartidaTorneos
                .Where(p => (p.IdUsuario1 == idUsuario || p.IdUsuario2 == idUsuario) &&
                            (p.IdTorneo == idTorneo))
                .ToListAsync();

            if (partidas == null) return [];
            return partidas;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al buscar las partidas", ex);
        }
    }

    public async Task<bool> Edit(PartidaTorneo partidaTorneo)
    {
        PartidaTorneo? existingEntity = await _dbamonsulContext.PartidaTorneos
            .FirstOrDefaultAsync(p => p.IdPartidaTorneo == partidaTorneo.IdPartidaTorneo);

        if (existingEntity == null) return false;

        // Actualiza las propiedades de la entidad existente
        _dbamonsulContext.Entry(existingEntity).CurrentValues.SetValues(partidaTorneo);

        await _dbamonsulContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> Register(PartidaTorneo partidaTorneo)
    {
        try
        {
            var response = await _dbamonsulContext.PartidaTorneos.AddAsync(partidaTorneo);
            if (response == null) return false;
            
            await _dbamonsulContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al crea la partida.", ex);
        }
    }
}