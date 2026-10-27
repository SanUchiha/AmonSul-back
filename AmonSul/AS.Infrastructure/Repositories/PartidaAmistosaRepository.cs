using AS.Domain.Models;
using AS.Infrastructure.Data;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class PartidaAmistosaRepository(DbamonsulContext dbamonsulContext) : IPartidaAmistosaRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<List<PartidaAmistosa>> GetPartidasAmistosas()
    {
        try
        {
            var response = await _dbamonsulContext.PartidaAmistosas.ToListAsync();
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor.", ex);
        }
    }
   
    public async Task<PartidaAmistosa> GetById(int id)
    {
        try
        {
            PartidaAmistosa? response = await _dbamonsulContext.PartidaAmistosas
                .AsNoTracking()
                .Where(x => x.IdPartidaAmistosa== id)
                .FirstOrDefaultAsync();
            return response!;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor.", ex);
        }
    }
  
    public async Task<bool> Register(PartidaAmistosa partidaAmistosa)
    {
        try
        {
            var response = await _dbamonsulContext.PartidaAmistosas.AddAsync(partidaAmistosa);
            if (response == null) return false;
            await _dbamonsulContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al crea la partida.", ex);
        }
    }
   
    public async Task<bool> Edit(PartidaAmistosa partidaAmistosa)
    {
        var existingEntity = await _dbamonsulContext.PartidaAmistosas
            .FirstOrDefaultAsync(p => p.IdPartidaAmistosa == partidaAmistosa.IdPartidaAmistosa);

        if (existingEntity == null) return false;

        // Actualiza las propiedades de la entidad existente
        _dbamonsulContext.Entry(existingEntity).CurrentValues.SetValues(partidaAmistosa);

        await _dbamonsulContext.SaveChangesAsync();

        return true;

    }

    public async Task<bool> Delete(int idPartida)
    {
        try
        {
            var partidaAmistosa = await _dbamonsulContext.PartidaAmistosas.FindAsync(idPartida);
            if (partidaAmistosa == null) return false;

            var existingEntity = _dbamonsulContext.ChangeTracker
                                    .Entries<PartidaAmistosa>()
                                    .FirstOrDefault(e => e.Entity.IdPartidaAmistosa == partidaAmistosa.IdPartidaAmistosa);

            if (existingEntity != null)
            {
                _dbamonsulContext.Entry(existingEntity.Entity).State = EntityState.Detached;
            }

            _dbamonsulContext.PartidaAmistosas.Remove(partidaAmistosa);
            await _dbamonsulContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema en el servidor al eliminar la partida.", ex);
        }
    }

    public async Task<List<PartidaAmistosa>> GetPartidaAmistosasByUsuario(string email)
    {
        try
        {

            List<PartidaAmistosa> rawPartidasAmistosas = await GetPartidasAmistosas();
            var idUsuario = await ConseguirIdUsuario(email);

            List<PartidaAmistosa> partidasAmistosasPorJugador = rawPartidasAmistosas.Where(p => p.IdUsuario1 == idUsuario || p.IdUsuario2 == idUsuario).ToList();

            if (partidasAmistosasPorJugador == null) return [];
            return partidasAmistosasPorJugador;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al buscar las partidas por jugador", ex);
        }
    }

    private async Task<int> ConseguirIdUsuario(string email)
    {
        try
        {
            var usuario = await _dbamonsulContext.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            return usuario == null ? throw new Exception("Usuario no encontrado") : usuario.IdUsuario;
        }
        catch (Exception ex)
        {
            throw new Exception($"{ex.Message}");
        }
    }

    public async Task<List<PartidaAmistosa>> GetPartidaAmistosasUsuarioById(int idUsuario)
    {
        try
        {
            // Proyección en lugar de Include: de los usuarios solo se usa el nick,
            // y el Include traía la Imagen (base64) de los dos jugadores en cada partida
            List<PartidaAmistosa> partidas = await _dbamonsulContext.PartidaAmistosas
                .AsNoTracking()
                .Where(p => p.IdUsuario1 == idUsuario || p.IdUsuario2 == idUsuario)
                .Select(p => new PartidaAmistosa
                {
                    IdPartidaAmistosa = p.IdPartidaAmistosa,
                    IdUsuario1 = p.IdUsuario1,
                    IdUsuario2 = p.IdUsuario2,
                    ResultadoUsuario1 = p.ResultadoUsuario1,
                    ResultadoUsuario2 = p.ResultadoUsuario2,
                    FechaPartida = p.FechaPartida,
                    EsMatchedPlayPartida = p.EsMatchedPlayPartida,
                    EscenarioPartida = p.EscenarioPartida,
                    PuntosPartida = p.PuntosPartida,
                    GanadorPartida = p.GanadorPartida,
                    EsElo = p.EsElo,
                    PartidaValidadaUsuario1 = p.PartidaValidadaUsuario1,
                    PartidaValidadaUsuario2 = p.PartidaValidadaUsuario2,
                    EsTorneo = p.EsTorneo,
                    EjercitoUsuario1 = p.EjercitoUsuario1,
                    EjercitoUsuario2 = p.EjercitoUsuario2,
                    IdUsuario1Navigation = p.IdUsuario1Navigation == null
                        ? null
                        : new Usuario
                        {
                            IdUsuario = p.IdUsuario1Navigation.IdUsuario,
                            Nick = p.IdUsuario1Navigation.Nick,
                        },
                    IdUsuario2Navigation = p.IdUsuario2Navigation == null
                        ? null
                        : new Usuario
                        {
                            IdUsuario = p.IdUsuario2Navigation.IdUsuario,
                            Nick = p.IdUsuario2Navigation.Nick,
                        },
                })
                .ToListAsync();

            if (partidas == null) return [];
            return partidas;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al buscar las partidas por jugador", ex);
        }
    }
}
