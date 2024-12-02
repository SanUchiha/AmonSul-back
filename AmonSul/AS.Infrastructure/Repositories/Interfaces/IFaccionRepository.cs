using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IFaccionRepository
{
    Task<List<Faccion>> GetFacciones();
    Task<Faccion> GetById(int? Id);
    Task<bool> Edit(Faccion faccion);
    Task<bool> Register(Faccion faccion);
    Task<bool> Delete(Faccion faccion);
}
