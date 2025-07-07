using AS.Application.DTOs.Torneo;
using AS.Domain.DTOs.Torneo;

namespace AS.Application.Interfaces;

public interface ITorneoApplication
{
    Task<List<TorneoDTO>> GetTorneos();
    Task<TorneoDTO> GetById(int Id);
    Task<bool> Register(CrearTorneoDTO torneoDTO);
    Task<bool> Delete(int id);
    Task<(byte[] FileBytes, string FileName)> GetBasesTorneo(int idTorneo);

    //Gestion
    Task<List<TorneoCreadoUsuarioDTO>> GetTorneosCreadosUsuario(int IdUsuario);
    Task<TorneoGestionInfoDTO> GetInfoTorneoCreado(int IdTorneo);
    Task<bool> UpdateTorneoAsync(UpdateTorneoDTO request);
    Task<bool> UpdateBasesTorneoAsync(UpdateBasesDTO request, int idTorneo);
    Task<TorneoEquipoGestionInfoDTO> GetInfoTorneoEquipoCreado(int idTorneo);
    Task<TorneoGestionInfoMasDTO> GetInfoTorneoCreadoMasAsync(int idTorneo);
    Task<bool?> HandlerMostrarListasAsync(HandlerMostrarListasDTO request, int idTorneo);
    Task<bool?> HandlerMostrarClasificacionAsync(HandlerMostrarClasificacionDTO request, int idTorneo);
}
