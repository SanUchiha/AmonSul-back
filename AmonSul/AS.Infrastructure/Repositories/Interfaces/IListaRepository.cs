using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IListaRepository
{
    Task<List<Lista>> GetListas();
    Task<List<Lista>> GetListasByUser(int idUsuario);
    Task<List<Lista>> GetListasByTorneo(int idTorneo);
    Task<Lista> GetListaInscripcionById(int IdInscripcion);
    Task<Lista> GetListaById(int IdLista);
    Task<Lista> Delete(int idLista);
    Task<bool> RegisterLista(Lista lista);
    Task<Lista> UpdateLista(Lista lista);
}
