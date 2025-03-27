using AS.Domain.DTOs.Lista;
using AS.Domain.Models;
using AS.Infrastructure.DTOs.Lista;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IListaRepository
{
    Task<List<Lista>> GetListas();
    Task<List<Lista>> GetListasByUser(int idUsuario);
    Task<List<Lista>> GetListasByTorneo(int idTorneo);
    Task<Lista> GetListaInscripcionById(int IdInscripcion);
    Task<Lista> GetListaById(int IdLista);
    Task<Lista> Delete(int idLista);
    Task<ResultRegisterListarDTO> RegisterLista(Lista lista);
    Task<Lista> UpdateLista(UpdateListaDTO updateListaTorneoDTO);
    Task<Lista> GetListaTorneo(int idTorneo, int idUsuario);
}
