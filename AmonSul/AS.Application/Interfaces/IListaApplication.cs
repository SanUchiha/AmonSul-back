using AS.Application.DTOs.Lista;
using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface IListaApplication
{
    Task<List<Lista>> GetListas();
    Task<List<Lista>> GetListasByUser(int idUsuario);
    Task<List<Lista>> GetListasByTorneo(int idTorneo);
    Task<ListaViewDTO> GetListaInscripcionById(int idInscripcion);
    Task<Lista> GetListaById(int idLista);
    Task<Lista> Delete(int idLista);
    Task<bool> RegisterLista(CreateListaTorneoDTO createListaTorneoDTO);
    Task<bool> UpdateLista(Lista lista);
    Task<string> GetListaTorneo(ListaTorneoRequestDTO listaTorneoRequestDTO);
}
