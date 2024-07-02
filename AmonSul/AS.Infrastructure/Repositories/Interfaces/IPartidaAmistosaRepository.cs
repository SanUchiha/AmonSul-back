using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IPartidaAmistosaRepository
{
    Task<List<PartidaAmistosa>> GetPartidasAmistosas();
    Task<PartidaAmistosa> GetById(int Id);
    Task<bool> Edit(PartidaAmistosa partidaAmistosa);
    Task<bool> Register(PartidaAmistosa partidaAmistosa);
    Task<bool> Delete(int id);
}
