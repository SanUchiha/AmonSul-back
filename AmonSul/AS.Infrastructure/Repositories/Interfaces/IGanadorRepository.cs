using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IGanadorRepository
{
    Task<Ganador> GetById(int id);
    Task<List<Ganador>> GetAll();
    Task<List<Ganador>> GetAllByUsuario(int idUsuario);
    Task<bool> Register(Ganador usuario);
    Task<bool> Delete(int id);

}
