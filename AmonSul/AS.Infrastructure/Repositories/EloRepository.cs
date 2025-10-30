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
            var response = await _dbamonsulContext.Elos.Where(e => e.IdUsuario == idUsuario).ToListAsync();
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

            if (response == null) return false;
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
            var response = await _dbamonsulContext.Elos.Where(e => e.IdElo == idElo).FirstOrDefaultAsync();

            if (response == null) return null!;
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al conseguir el Elo por el Id.", ex);
        }
    }

    public async Task<bool> CheckEloByUser(int idUsuario)
    {
        bool response = 
            await _dbamonsulContext.Elos.AnyAsync(x => x.IdUsuario == idUsuario);

        if (response) return true;

        Elo elo = new()
        {
            IdUsuario = idUsuario,
            FechaElo = DateTime.Now,
            PuntuacionElo = 800
        };

        await _dbamonsulContext.Elos.AddAsync(elo);
        await _dbamonsulContext.SaveChangesAsync();

        return true;
    }
}
    
