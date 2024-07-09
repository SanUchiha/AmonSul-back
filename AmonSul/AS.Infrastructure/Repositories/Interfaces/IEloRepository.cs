using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IEloRepository
{
    Task<List<Elo>> GetElos();
    Task<List<Elo>> GetElosByIdUser(int idUsuario);
    Task<Elo> GetEloById(int idElo);
    Task<bool> Edit(Elo elo);
    Task<bool> RegisterElo(Elo elo);
    Task<bool> Delete(int idElo);
}
