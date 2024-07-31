using AS.Application.DTOs.PartidaAmistosa;

namespace AS.Application.Interfaces;

public interface IPartidaAmistosaApplication
{
    Task<List<ViewPartidaAmistosaDTO>> GetPartidasAmistosas();
    Task<List<ViewPartidaAmistosaDTO>> GetPartidasAmistosasValidadas();
    Task<ViewPartidaAmistosaDTO> GetById(int Id);
    Task<List<ViewPartidaAmistosaDTO>> GetPartidaAmistosasByUsuario(string email);
    Task<List<ViewPartidaAmistosaDTO>> GetPartidaAmistosasByUsuarioValidadas(int idUsuario);
    Task<List<ViewPartidaAmistosaDTO>> GetPartidaAmistosasByUsuarioPendientes(int idUsuario);
    Task<bool> Edit(UpdatePartidaAmistosaDTO partidaAmistosa);
    Task<bool> Register(CreatePartidaAmistosaDTO partidaAmistosa);
    Task<bool> Delete(int id);
    Task<bool> ValidarPartidaAmistosa(ValidarPartidaDTO validarPartidaDTO);
}
