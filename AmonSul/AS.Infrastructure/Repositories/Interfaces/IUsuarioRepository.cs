using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario> GetById(int IdUsuario);
        Task<bool> Edit(Usuario usuario);
        Task<bool> Register(Usuario usuario);
        Task<bool> Delete(Usuario usuario);
    }
}
