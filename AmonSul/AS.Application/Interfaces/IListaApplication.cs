using AS.Application.DTOs.Lista;
using AS.Domain.DTOs.Lista;
using AS.Domain.Models;
using AS.Infrastructure.DTOs.Lista;

namespace AS.Application.Interfaces;

public interface IListaApplication
{
    Task<List<Lista>> GetListas(int idUsuarioSolicitante);
    Task<List<Lista>> GetListasByUser(int idUsuario, int idUsuarioSolicitante);
    Task<List<Lista>> GetListasByTorneo(int idTorneo);
    Task<ListaViewDTO> GetListaInscripcionById(int idInscripcion);
    Task<ListaDTO> GetListaById(int idLista);
    Task<Lista> Delete(int idLista);
    Task<ResultRegisterListarDTO> RegisterLista(CreateListaTorneoDTO createListaTorneoDTO);
    Task<bool> UpdateLista(UpdateListaDTO updateListaTorneoDTO);
    Task<ListaDTO> GetListaTorneo(ListaTorneoRequestDTO listaTorneoRequestDTO);
    Task<bool> PuedeVerListaTorneoAsync(int idTorneo, int idUsuarioLista, int idUsuarioSolicitante);
    Task<bool> PuedeVerListaAsync(int idLista, int idUsuarioSolicitante);
    Task<bool> PuedeVerListasInscripcionAsync(int idInscripcion, int idUsuarioSolicitante);
    Task<bool> PuedeVerListasTorneoAsync(int idTorneo, int idUsuarioSolicitante);
    Task<List<ListaViewDTO>> GetListasByInscripcionAsync(int idInscripcion);
    Task<bool> UpdateEstadoLista(UpdateEstadoListaDTO request);

    Task<(byte[] FileBytes, string FileName)> GetListasPdfByTorneo(int idTorneo);
    Task<(Stream Stream, string FileName)> GetListasPdfStreamByTorneo(int idTorneo);
    Task<List<ListaCompletaDTO>> GetListasCompletasByTorneo(int idTorneo);
    Task<MigracionCloudinaryResultDTO> MigrarListasACloudinaryAsync();
}
