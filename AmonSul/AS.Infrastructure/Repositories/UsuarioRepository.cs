using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;

namespace AS.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DbamonsulContext _dbamonsulContext;

        public UsuarioRepository(DbamonsulContext dbamonsulContext, Utilidades utilidades)
        {
            _dbamonsulContext = dbamonsulContext;
        }

        public async Task<bool> Register(Usuario usuario)
        {
            await _dbamonsulContext.AddAsync(usuario);
            await _dbamonsulContext.SaveChangesAsync();

            return true;
        }

        public Task<bool> Delete(Usuario usuario)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Edit(Usuario usuario)
        {
            throw new NotImplementedException();
        }

        public Task<Usuario> GetById(int IdUsuario)
        {
            throw new NotImplementedException();
        }
    }
}