using AS.Application.DTOs;

namespace AS.Application.Interfaces
{
    public interface IUsuarioApplication
    {
        Task<UsuarioDTO> GetById(int IdUsuario);
        Task<bool> Edit(EditarUsuarioDTO usuario);
        Task<bool> Register(RegistrarUsuarioDTO usuario);
        Task<bool> Delete(EliminarUsuarioDTO usuario);
    }
}
