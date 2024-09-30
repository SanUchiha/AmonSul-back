using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface IGanadorApplication
{
    Task<Ganador> GetById(int id);
    Task<List<Ganador>> GetAll();
    Task<bool> Register(Ganador usuario);
    Task<bool> Delete(int id);
}
