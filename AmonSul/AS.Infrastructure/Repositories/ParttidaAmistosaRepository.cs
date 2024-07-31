using AS.Domain.Models;
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
            var response = await _dbamonsulContext.PartidaAmistosas
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
            List<PartidaAmistosa> partidas = await _dbamonsulContext.PartidaAmistosas
                .Where(p => p.IdUsuario1 == idUsuario || p.IdUsuario2 == idUsuario)
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
