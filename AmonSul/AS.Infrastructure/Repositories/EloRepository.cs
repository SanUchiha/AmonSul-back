using AS.Domain.DTOs.Elos;
using AS.Domain.Models;
using AS.Infrastructure.Data;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class EloRepository(DbamonsulContext dbamonsulContext) : IEloRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<List<Elo>> GetElos()
    {
        try
        {
            var response = await _dbamonsulContext.Elos.ToListAsync();
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al conseguir los elos.", ex);
        }
    }

    public async Task<List<Elo>> GetElosByIdUser(int idUsuario)
    {
        try
        {
            var response = await _dbamonsulContext
                .Elos.Where(e => e.IdUsuario == idUsuario)
                .ToListAsync();
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al conseguir los elos.", ex);
        }
    }

    public async Task<bool> RegisterElo(Elo elo)
    {
        try
        {
            var response = await _dbamonsulContext.Elos.AddAsync(elo);
            await _dbamonsulContext.SaveChangesAsync();

            if (response == null)
                return false;
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al conseguir los elos.", ex);
        }
    }

    public Task<bool> Delete(int idElo)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Edit(Elo elo)
    {
        throw new NotImplementedException();
    }

    public async Task<Elo> GetEloById(int idElo)
    {
        try
        {
            var response = await _dbamonsulContext
                .Elos.Where(e => e.IdElo == idElo)
                .FirstOrDefaultAsync();

            if (response == null)
                return null!;
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Ocurrio un problema en el servidor al conseguir el Elo por el Id.",
                ex
            );
        }
    }

    public async Task<bool> CheckEloByUser(int idUsuario)
    {
        bool response = await _dbamonsulContext.Elos.AnyAsync(x => x.IdUsuario == idUsuario);

        if (response)
            return true;

        Elo elo = new()
        {
            IdUsuario = idUsuario,
            FechaElo = DateTime.Now,
            PuntuacionElo = 800,
        };

        await _dbamonsulContext.Elos.AddAsync(elo);
        await _dbamonsulContext.SaveChangesAsync();

        return true;
    }

    public async Task<List<Usuario>> GetUsuariosWithElos()
    {
        try
        {
            return await _dbamonsulContext.Usuarios.Include(u => u.Elos).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener los usuarios con sus elos.", ex);
        }
    }

    /// <summary>
    /// Variación de ELO de cada jugador en las partidas dadas: su ELO tras la partida
    /// menos el anterior (fila previa por IdElo, o 800 si no tenía, como GetLastElo)
    /// </summary>
    public async Task<List<EloVariacionPartidaDTO>> GetVariacionesByPartidas(List<int> idsPartidaTorneo) =>
        await _dbamonsulContext.Elos
            .AsNoTracking()
            .Where(e =>
                e.IdPartidaTorneo != null
                && idsPartidaTorneo.Contains(e.IdPartidaTorneo.Value)
                && e.IdUsuario != null
                && e.PuntuacionElo != null
            )
            .Select(e => new EloVariacionPartidaDTO
            {
                IdPartidaTorneo = e.IdPartidaTorneo!.Value,
                IdUsuario = e.IdUsuario!.Value,
                Variacion = e.PuntuacionElo!.Value
                    - (
                        _dbamonsulContext.Elos
                            .Where(p => p.IdUsuario == e.IdUsuario && p.IdElo < e.IdElo)
                            .OrderByDescending(p => p.IdElo)
                            .Select(p => p.PuntuacionElo)
                            .FirstOrDefault() ?? 800
                    ),
            })
            .ToListAsync();

    public async Task<List<ClasificacionEloDataDTO>> GetClasificacionEloAsync()
    {
        return await _dbamonsulContext.Usuarios
            .Where(u => u.IdUsuario != 568)
            .Select(u => new ClasificacionEloDataDTO
            {
                IdUsuario = u.IdUsuario,
                Nick = u.Nick,
                IdFaccion = u.IdFaccion,
                Elo = u.Elos
                    .OrderByDescending(e => e.FechaElo)
                    .Select(e => e.PuntuacionElo)
                    .FirstOrDefault() ?? 800,
            })
            .ToListAsync();
    }

    /// <summary>
    /// Filas de ELO de una partida de torneo, para corregirlas o quitarlas
    /// </summary>
    public async Task<List<Elo>> GetByPartidaAsync(int idPartidaTorneo) =>
        await _dbamonsulContext.Elos.Where(e => e.IdPartidaTorneo == idPartidaTorneo).ToListAsync();

    /// <summary>
    /// ELO actual del jugador: el de su fila más reciente, o 800 si no tiene, como GetLastElo
    /// </summary>
    public async Task<int> GetUltimoEloAsync(int idUsuario) =>
        await _dbamonsulContext.Elos
            .Where(e => e.IdUsuario == idUsuario)
            .OrderByDescending(e => e.FechaElo)
            .ThenByDescending(e => e.IdElo)
            .Select(e => e.PuntuacionElo)
            .FirstOrDefaultAsync() ?? 800;

    /// <summary>
    /// ELO del jugador justo antes de una de sus filas: la anterior por IdElo, o 800, como GetVariacionesByPartidas
    /// </summary>
    public async Task<int> GetEloAnteriorAsync(int idUsuario, int idElo) =>
        await _dbamonsulContext.Elos
            .Where(e => e.IdUsuario == idUsuario && e.IdElo < idElo)
            .OrderByDescending(e => e.IdElo)
            .Select(e => e.PuntuacionElo)
            .FirstOrDefaultAsync() ?? 800;

    /// <summary>
    /// Suma la diferencia a todas las filas del jugador posteriores a una. Cada fila guarda el ELO tras la partida,
    /// así que corregir o quitar una partida antigua mueve igual el resto de su historial. No guarda
    /// </summary>
    public async Task DesplazarPosterioresAsync(int idUsuario, int idElo, int diferencia)
    {
        List<Elo> posteriores = await _dbamonsulContext.Elos
            .Where(e => e.IdUsuario == idUsuario && e.IdElo > idElo)
            .ToListAsync();
        foreach (Elo elo in posteriores)
            elo.PuntuacionElo += diferencia;
    }

    public void Agregar(Elo elo) => _dbamonsulContext.Elos.Add(elo);

    public void Quitar(Elo elo) => _dbamonsulContext.Elos.Remove(elo);
}
