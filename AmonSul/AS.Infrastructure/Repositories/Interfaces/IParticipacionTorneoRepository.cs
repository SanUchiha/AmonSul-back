using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IParticipacionTorneoRepository
{
    Task<ParticipacionTorneo?> GetById(int id);
    Task<ParticipacionTorneo?> GetByIdUsuarioAndRonda(int idTorneo, int idUsuario, int IdRonda);
    Task<ParticipacionTorneo> Register(ParticipacionTorneo participacionTorneo);
}
