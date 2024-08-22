using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.Usuario;

namespace AS.Application.Interfaces;

public interface IUsuarioApplication
{
    Task<UsuarioDTO> GetById(int IdUsuario);
    Task<bool> Edit(EditarUsuarioDTO usuario);
    Task<bool> ModificarFaccion(EditarFaccionUsuarioDTO editarFaccionUsuarioDTO);
    Task<RegistrarUsuarioResponseDTO> Register(RegistrarUsuarioDTO usuario);
    Task<bool> CambiarPass(CambiarPassDTO cambiarPassDTO);
    Task<bool> RecordarPass(string email);
    Task<ViewUsuarioPartidaDTO> GetByEmail(string email);
    Task<UsuarioDTO> GetByNick(string nick);
    Task<List<ViewUsuarioPartidaDTO>> GetAll();
    Task<List<UsuarioDTO>> GetUsuarios();

    Task<List<UsuarioNickDTO>> GetNicks();

    Task<bool> Delete(string email);
    Task<UsuarioViewDTO> GetUsuario(string email);
    Task<string> GetNickById(int idUsuario);
    Task<ViewDetalleUsuarioDTO> GetDetalleUsuarioByEmail(string email);

    Task<UsuarioDataDTO> GetUsuarioData(int idUsuario);


}
