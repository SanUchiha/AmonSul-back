using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class ParticipacionTorneoRepository(DbamonsulContext dbamonsulContext) : IParticipacionTorneoRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<ParticipacionTorneo?> GetByIdUsuarioAndRonda(int idTorneo, int idUsuario, int IdRonda) => 
        await _dbamonsulContext.ParticipacionTorneos.FirstOrDefaultAsync(p =>
            p.IdTorneo == idTorneo &&
            p.IdUsuario == idUsuario &&
            p.IdRonda == IdRonda);

    public async Task<ParticipacionTorneo?> GetById(int id) =>
        await _dbamonsulContext.ParticipacionTorneos.FirstOrDefaultAsync(p =>
            p.IdParticipacionTorneo == id);

    public async Task<ParticipacionTorneo> Register(ParticipacionTorneo participacionTorneo)
    {
        _dbamonsulContext.ParticipacionTorneos.Add(participacionTorneo);
        await _dbamonsulContext.SaveChangesAsync();
        return participacionTorneo;
    }

}