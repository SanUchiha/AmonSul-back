using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface ITorneoRepository
{
    Task<List<Torneo>> GetTorneos();
    Task<List<Torneo>> GetTorneosCreadosUsuario(int idUsuario);
    Task<Torneo> GetById(int Id);
    Task<bool> Edit(Torneo torneo);
    Task<bool> Register(Torneo torneo);
    Task<bool> Delete(int id);
}
