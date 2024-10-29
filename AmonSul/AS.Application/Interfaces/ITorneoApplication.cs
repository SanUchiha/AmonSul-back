using AS.Application.DTOs.Torneo;
using AS.Domain.DTOs.Torneo;

namespace AS.Application.Interfaces;

public interface ITorneoApplication
{
    Task<List<TorneoDTO>> GetTorneos();
    Task<TorneoDTO> GetById(int Id);
    Task<bool> Edit(TorneoDTO torneoDTO);
    Task<bool> Register(CrearTorneoDTO torneoDTO);
    Task<bool> Delete(int id);
    Task<(byte[] FileBytes, string FileName)> GetBasesTorneo(int idTorneo);

    //Gestion
    Task<List<TorneoCreadoUsuarioDTO>> GetTorneosCreadosUsuario(int IdUsuario);
    Task<TorneoGestionInfoDTO> GetInfoTorneoCreado(int IdTorneo);
}
