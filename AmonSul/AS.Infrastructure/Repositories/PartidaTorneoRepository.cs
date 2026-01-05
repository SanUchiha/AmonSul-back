using AS.Domain.DTOs.Elos;
using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;
using AS.Infrastructure.Data;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class PartidaTorneoRepository(DbamonsulContext dbamonsulContext) : IPartidaTorneoRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<List<PartidaTorneo>> GetByFechaAsync(DateTime fecha)
    {
        return await _dbamonsulContext
            .PartidaTorneos.Where(p =>
                p.FechaPartida != null && p.FechaPartida.Value == DateOnly.FromDateTime(fecha)
            )
            .ToListAsync();
    }

    public async Task<List<PartidaTorneo>> GetAllAsync()
    {
        return await _dbamonsulContext.PartidaTorneos.ToListAsync();
    }

    public async Task<bool> Delete(int idPartida)
    {
        try
        {
            PartidaTorneo? partida = await _dbamonsulContext.PartidaTorneos.FindAsync(idPartida);
            if (partida == null)
                return false;

            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PartidaTorneo>? existingEntity =
                _dbamonsulContext
                    .ChangeTracker.Entries<PartidaTorneo>()
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
            PartidaTorneo? response = await _dbamonsulContext
                .PartidaTorneos.AsNoTracking()
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
            List<PartidaTorneo> partidas = await _dbamonsulContext
                .PartidaTorneos.Where(p => p.IdTorneo == idTorneo)
                .Include(p => p.IdUsuario1Navigation)
                .Include(p => p.IdUsuario2Navigation)
                .Include(p => p.IdEquipo1Navigation)
                    .ThenInclude(e => e!.InscripcionTorneos)
                .Include(p => p.IdEquipo2Navigation)
                    .ThenInclude(e => e!.InscripcionTorneos)
                .ToListAsync();

            if (partidas == null)
                return [];
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
            List<PartidaTorneo> partidas = await _dbamonsulContext
                .PartidaTorneos.Where(p => p.IdTorneo == idTorneo && p.NumeroRonda == ronda)
                .ToListAsync();

            if (partidas == null)
                return [];
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
            List<PartidaTorneo> partidas = await _dbamonsulContext
                .PartidaTorneos.Where(p => p.IdUsuario1 == idUsuario || p.IdUsuario2 == idUsuario)
                .Include(p => p.IdUsuario1Navigation)
                .Include(p => p.IdUsuario2Navigation)
                .Include(p => p.IdTorneoNavigation)
                .ToListAsync();

            if (partidas == null)
                return [];
            return partidas;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al buscar las partidas", ex);
        }
    }

    public async Task<List<PartidaTorneo>> GetPartidasTorneoByUsuarioAsync(
        int idTorneo,
        int idUsuario
    )
    {
        try
        {
            List<PartidaTorneo> partidas = await _dbamonsulContext
                .PartidaTorneos.AsNoTracking()
                .Where(p =>
                    (p.IdUsuario1 == idUsuario || p.IdUsuario2 == idUsuario)
                    && (p.IdTorneo == idTorneo)
                )
                .Include(p => p.IdUsuario1Navigation)
                .Include(p => p.IdUsuario2Navigation)
                .ToListAsync();

            if (partidas == null)
                return [];
            return partidas;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al buscar las partidas", ex);
        }
    }

    public async Task<bool> Edit(PartidaTorneo partidaTorneo)
    {
        using var transaction = await _dbamonsulContext.Database.BeginTransactionAsync();

        PartidaTorneo? existingEntity = await _dbamonsulContext
            .PartidaTorneos.Where(p => p.IdPartidaTorneo == partidaTorneo.IdPartidaTorneo)
            .FirstOrDefaultAsync();

        if (existingEntity == null)
            return false;

        _dbamonsulContext.Entry(existingEntity).CurrentValues.SetValues(partidaTorneo);

        if (existingEntity.ResultadoUsuario1 != null && existingEntity.ResultadoUsuario2 != null)
        {
            if (existingEntity.ResultadoUsuario1 == existingEntity.ResultadoUsuario2)
                existingEntity.GanadorPartidaTorneo = null;
            else
            {
                if (existingEntity.ResultadoUsuario1 > existingEntity.ResultadoUsuario2)
                    existingEntity.GanadorPartidaTorneo = existingEntity.IdUsuario1;
                else
                    existingEntity.GanadorPartidaTorneo = existingEntity.IdUsuario2;
            }
        }
        await _dbamonsulContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    public async Task<bool> Register(PartidaTorneo partidaTorneo)
    {
        try
        {
            var response = await _dbamonsulContext.PartidaTorneos.AddAsync(partidaTorneo);
            if (response == null)
                return false;

            await _dbamonsulContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al crea la partida.", ex);
        }
    }

    public async Task<bool> GenerateRound(List<PartidaTorneo> partidasRonda)
    {
        try
        {
            foreach (var item in partidasRonda)
            {
                var response = await _dbamonsulContext.PartidaTorneos.AddAsync(item);
                if (response == null)
                    return false;

                await _dbamonsulContext.SaveChangesAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Ocurrio un problema en el servidor al generar las partidas de la ronda.",
                ex
            );
        }
    }

    public async Task<List<UpdateEloPartidaDTO>> GetPartidasTorneoByRondaForEloAsync(
        int idTorneo,
        int idRonda
    )
    {
        try
        {
            List<UpdateEloPartidaDTO> partidas = await _dbamonsulContext
                .PartidaTorneos.Where(p => p.IdTorneo == idTorneo && p.NumeroRonda == idRonda)
                .Select(p => new UpdateEloPartidaDTO
                {
                    IdUsuario1 = p.IdUsuario1,
                    IdUsuario2 = p.IdUsuario2,
                    GanadorPartidaTorneo = p.GanadorPartidaTorneo,
                })
                .ToListAsync();

            if (partidas == null)
                return [];

            return partidas;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al buscar las partidas", ex);
        }
    }

    public async Task<bool> RegisterMany(List<PartidaTorneo> partidaTorneos)
    {
        await _dbamonsulContext.PartidaTorneos.AddRangeAsync(partidaTorneos);
        await _dbamonsulContext.SaveChangesAsync();

        return true;
    }

    public async Task<List<PartidaTorneoDTO>> GetPartidasTorneoAsync(int idTorneo)
    {
        List<PartidaTorneoDTO> partidas = await _dbamonsulContext
            .PartidaTorneos.Where(p => p.IdTorneo == idTorneo)
            .Select(p => new PartidaTorneoDTO
            {
                IdPartidaTorneo = p.IdPartidaTorneo,
                IdTorneo = p.IdTorneo,
                IdUsuario1 = p.IdUsuario1,
                IdUsuario2 = p.IdUsuario2,
                ResultadoUsuario1 = p.ResultadoUsuario1,
                ResultadoUsuario2 = p.ResultadoUsuario2,
                FechaPartida = p.FechaPartida,
                EscenarioPartida = p.EscenarioPartida,
                GanadorPartidaTorneo = p.GanadorPartidaTorneo,
                PartidaValidadaUsuario1 = p.PartidaValidadaUsuario1,
                PartidaValidadaUsuario2 = p.PartidaValidadaUsuario2,
                EjercitoUsuario1 = p.EjercitoUsuario1,
                EjercitoUsuario2 = p.EjercitoUsuario2,
                NumeroRonda = p.NumeroRonda,
                LiderMuertoUsuario1 = p.LiderMuertoUsuario1,
                LiderMuertoUsuario2 = p.LiderMuertoUsuario2,
                IdEquipo1 = p.IdEquipo1,
                IdEquipo2 = p.IdEquipo2,
                Nick1 = p.IdUsuario1Navigation!.Nick,
                Nick2 = p.IdUsuario2Navigation!.Nick,
                NombreEquipo1 = p.IdEquipo1Navigation!.NombreEquipo,
                NombreEquipo2 = p.IdEquipo2Navigation!.NombreEquipo,
                IdCapitan1 = p.IdEquipo1Navigation!.IdCapitan,
                IdCapitan2 = p.IdEquipo2Navigation!.IdCapitan,
            })
            .ToListAsync();

        return partidas;
    }

    public async Task<List<PartidaTorneo>> GetPartidasTorneoEquiposParaModificarAsync(
        int idEquipo1Old,
        int idEquipo2Old,
        int idTorneo,
        int numeroRonda
    )
    {
        List<PartidaTorneo> partidas = await _dbamonsulContext
            .PartidaTorneos.Where(p => p.IdTorneo == idTorneo)
            .Where(p => p.IdEquipo1 == idEquipo1Old && p.IdEquipo2 == idEquipo2Old)
            .Where(p => p.NumeroRonda == numeroRonda)
            .ToListAsync();

        return partidas;
    }

    public async Task<List<PartidaTorneo>> GetByFechaYUsuarioAsync(DateTime fecha, int idUsuario)
    {
        return await _dbamonsulContext
            .PartidaTorneos.Where(p =>
                p.FechaPartida != null
                && p.FechaPartida.Value == DateOnly.FromDateTime(fecha)
                && (p.IdUsuario1 == idUsuario || p.IdUsuario2 == idUsuario)
            )
            .ToListAsync();
    }

    public async Task<List<PartidaTorneo>> GetPartidasByEquipoYFechaAsync(
        int? idEquipo,
        DateTime fecha)
    {
        return await _dbamonsulContext
            .PartidaTorneos.Where(p =>
                p.FechaPartida != null
                && p.FechaPartida.Value == DateOnly.FromDateTime(fecha)
                && (p.IdEquipo1 == idEquipo || p.IdEquipo2 == idEquipo)
            )
            .ToListAsync();
    }
}
