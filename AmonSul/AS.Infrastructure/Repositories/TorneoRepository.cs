using AS.Domain.DTOs.Ganador;
using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;
using AS.Infrastructure.DTOs;
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
            return await _dbamonsulContext.Torneos.ToListAsync();
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

    public async Task<ResultTorneoCreadoDTO> Register(Torneo torneo)
    {
        try
        {
            await _dbamonsulContext.Torneos.AddAsync(torneo);
            await _dbamonsulContext.SaveChangesAsync();

            return new ResultTorneoCreadoDTO
            {
                IdTorneo = torneo.IdTorneo,
                HasCreated = true
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema en el servidor al crear el torneo.", ex);
        }
    }


    public async Task<bool> Edit(Torneo torneo)
    {
        try
        {
            var isUpdate = _dbamonsulContext.Torneos.Update(torneo);
            if (isUpdate == null) return false;
            await _dbamonsulContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor al modificar el torneo.", ex);
        }
    }

    public async Task<bool> Delete(int id)
    {
        Torneo? torneo = await _dbamonsulContext.Torneos.FindAsync(id);

        if (torneo == null) return false;

        _dbamonsulContext.Torneos.Remove(torneo);
        await _dbamonsulContext.SaveChangesAsync();

        return true;
    }

    public async Task<List<TorneoCreadoUsuarioDTO>> GetTorneosCreadosUsuario(int idUsuario) =>
        await _dbamonsulContext.Torneos
            .Where(t => t.IdUsuario == idUsuario)
            .Select(t => new TorneoCreadoUsuarioDTO
            {
                IdTorneo = t.IdTorneo,
                IdUsuario = t.IdUsuario,
                NombreTorneo = t.NombreTorneo,
                TipoTorneo = t.TipoTorneo,
                ListasPorJugador = t.ListasPorJugador
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

