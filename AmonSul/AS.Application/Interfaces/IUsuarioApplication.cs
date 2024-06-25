using AS.Application.DTOs;
using AS.Application.DTOs.Usuario;

namespace AS.Application.Interfaces
{
    public interface IUsuarioApplication
    {
        Task<UsuarioDTO> GetById(int IdUsuario);
        Task<bool> Edit(EditarUsuarioDTO usuario);
        Task<RegistrarUsuarioResponseDTO> Register(RegistrarUsuarioDTO usuario);
        Task<bool> Delete(EliminarUsuarioDTO usuario);
    }
}
