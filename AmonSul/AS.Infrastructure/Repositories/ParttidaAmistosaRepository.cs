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
            var response = await _dbamonsulContext.PartidaAmistosas.AsNoTracking().Where(x => x.IdPartidaAmistosa== id).FirstOrDefaultAsync();
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
        try
        {
            var response = _dbamonsulContext.PartidaAmistosas.Update(partidaAmistosa);
            if (response == null) return false;
            await _dbamonsulContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al actualizar la partida.", ex);
        }
    }
 
    public Task<bool> Delete(int id)
    {
        throw new NotImplementedException();
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
}
