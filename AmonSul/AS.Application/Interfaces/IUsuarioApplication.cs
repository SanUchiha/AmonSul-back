using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.Usuario;
using AS.Domain.DTOs.Usuario;

namespace AS.Application.Interfaces;

public interface IUsuarioApplication
{
    Task<UsuarioDTO> GetById(int IdUsuario);
    Task<bool> EditAsync(EditarUsuarioDTO usuario);
    Task<bool> ModificarFaccion(EditarFaccionUsuarioDTO editarFaccionUsuarioDTO);
    Task<RegistrarUsuarioResponseDTO> Register(RegistrarUsuarioDTO usuario);
    Task<bool> CambiarPass(CambiarPassDTO cambiarPassDTO);
    Task PedirEnlaceContrasena(string email);
    Task<bool> RestablecerContrasena(string enlace, string nuevaContrasena);
    Task<ViewUsuarioPartidaDTO> GetByEmail(string email);
    Task<UsuarioDTO> GetByNick(string nick);
    Task<List<ViewUsuarioPartidaDTO>> GetAll();
    Task<List<UsuarioDTO>> GetUsuarios();
    Task<List<UsuarioNickDTO>> GetNicks();
    Task<(bool Ok, string? Error)> BorrarMiCuentaAsync(int idUsuario, string contrasena);
    Task<(bool Ok, string? Error)> BorrarCuentaAsync(int idUsuario);
    Task<int?> GetIdByEmailAsync(string email);
    Task<UsuarioViewDTO> GetUsuario(string email);
    Task<string> GetNickById(int idUsuario);
    Task<ViewDetalleUsuarioDTO> GetDetalleUsuarioByEmail(string email);
    Task<UsuarioDataDTO> GetUsuarioDataAsync(int idUsuario);
    Task<bool> UpdateProteccionDatos(UpdateProteccionDatosDTO updateProteccionDatosDTO);
    Task<List<UsuarioSinEquipoDTO>> GetUsuariosNoInscritosTorneoAsync(int idTorneo);
    Task<List<UsuarioInscripcionTorneoDTO>> GetUsuariosByTorneo(int idTorneo);
    Task<bool> GetProteccionDatos(int idUsuario);
    Task<List<UsuarioIdNickDTO>> GetAllIdNickAsync();
}
