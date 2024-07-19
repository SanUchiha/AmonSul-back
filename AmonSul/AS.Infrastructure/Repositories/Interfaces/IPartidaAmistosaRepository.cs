using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IPartidaAmistosaRepository
{
    Task<List<PartidaAmistosa>> GetPartidasAmistosas();
    Task<PartidaAmistosa> GetById(int Id);
    Task<List<PartidaAmistosa>> GetPartidaAmistosasByUsuario(string email);
    Task<bool> Edit(PartidaAmistosa partidaAmistosa);
    Task<bool> Register(PartidaAmistosa partidaAmistosa);
    Task<bool> Delete(PartidaAmistosa partidaAmistosa);
}
