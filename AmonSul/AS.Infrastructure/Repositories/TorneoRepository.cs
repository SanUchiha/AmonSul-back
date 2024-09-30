using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class TorneoRepository : ITorneoRepository
{
    private readonly DbamonsulContext _dbamonsulContext;

    public TorneoRepository(DbamonsulContext dbamonsulContext)
    {
        _dbamonsulContext = dbamonsulContext;
    }

    public async Task<List<Torneo>> GetTorneos()
    {
        try
        {
            var response = await _dbamonsulContext.Torneos.ToListAsync();
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor.", ex);
        }
    }

    public async Task<Torneo> GetById(int Id)
    {
        try
        {
            var response = await _dbamonsulContext.Torneos
                              .AsNoTracking()
                              .FirstOrDefaultAsync(t => t.IdTorneo == Id);
         
            return response!;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor.", ex);
        }
    }

    public async Task<TorneoUsuarioDto> GetNombreById(int idTorneo)
    {
        try
        {
            TorneoUsuarioDto? response = await _dbamonsulContext.Torneos
                              .AsNoTracking()
                              .Where(t => t.IdTorneo == idTorneo)
                              .Select(t => new TorneoUsuarioDto
                              {
                                  NombreTorneo = t.NombreTorneo,
                                  IdUsuario = t.IdUsuario
                              })
                              .FirstOrDefaultAsync();

            return response ?? throw new Exception("Torneo no encontrado");
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema en el servidor.", ex);
        }
    }

    public async Task<bool> Register(Torneo torneo)
    {
        try
        {
            var response = await _dbamonsulContext.Torneos.AddAsync(torneo);
            if (response == null) return false;
            await _dbamonsulContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al crea la partida.", ex);
        }
    }

    public Task<bool> Edit(Torneo torneo)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Torneo>> GetTorneosCreadosUsuario(int idUsuario)
    {
        try
        {
            var response = await _dbamonsulContext.Torneos
                .Include(t => t.InscripcionTorneos)
                .Where(t => t.IdUsuario == idUsuario)
                .ToListAsync();
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor.", ex);
        }
    }
}

