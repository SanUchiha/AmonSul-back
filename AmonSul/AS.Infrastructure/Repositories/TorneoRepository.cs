using AS.Domain.DTOs.Ganador;
using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class TorneoRepository(DbamonsulContext dbamonsulContext) : ITorneoRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

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
            Torneo? response = await _dbamonsulContext.Torneos
                              .AsNoTracking()
                              .FirstOrDefaultAsync(t => t.IdTorneo == Id);
         
            return response!;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor.", ex);
        }
    }

    public async Task<int> GetIdOrganizadorByIdTorneo(int IdTorneo) =>
        await _dbamonsulContext.Torneos
           .Where(u => u.IdTorneo == IdTorneo)
           .Select(u => u.IdUsuario)
           .FirstOrDefaultAsync();

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
            throw new Exception("Ocurrio un problema en el servidor al crea el torneo.", ex);
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

    public async Task<List<TorneoCreadoUsuarioDTO>> GetTorneosCreadosUsuario(int idUsuario) =>
        await _dbamonsulContext.Torneos
            .Where(t => t.IdUsuario == idUsuario)
            .Select(t => new TorneoCreadoUsuarioDTO
            {
                IdTorneo = t.IdTorneo,
                IdUsuario = t.IdUsuario,
                NombreTorneo = t.NombreTorneo
            })
            .ToListAsync();

    public async Task<List<GanadorTorneoDTO>> GetAllSoloNames()
    {
        try
        {
            List<GanadorTorneoDTO> response = await _dbamonsulContext.Torneos
                .Select(t => new GanadorTorneoDTO
                {
                    IdTorneo = t.IdTorneo,
                    TournamentName = t.NombreTorneo
                })
                .ToListAsync();

            return response;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al conseguir los torneos.", ex);
        }
    }
}

