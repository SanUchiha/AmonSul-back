using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.Usuario;

namespace AS.Application.Interfaces;

public interface IUsuarioApplication
{
    //Task<UsuarioDTO> GetById(int IdUsuario);
    Task<bool> Edit(EditarUsuarioDTO usuario);
    Task<RegistrarUsuarioResponseDTO> Register(RegistrarUsuarioDTO usuario);
    Task<ViewUsuarioPartidaDTO> GetByEmail(string email);
    Task<UsuarioDTO> GetByNick(string nick);
    Task<List<ViewUsuarioPartidaDTO>> GetAll();
    Task<bool> Delete(string email);
    Task<UsuarioViewDTO> GetUsuario(string email);
    Task<string> GetNickById(int idUsuario);
}
