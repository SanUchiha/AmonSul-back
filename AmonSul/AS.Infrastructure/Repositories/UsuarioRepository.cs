using AS.Domain.Exceptions;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

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
            try
            {
                await _dbamonsulContext.AddAsync(usuario);
                await _dbamonsulContext.SaveChangesAsync();
                return true;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                if (ex.Message.Contains("UQ__Usuario__7D3471B6867373C0"))
                {
                    throw new UniqueKeyViolationException("El nick ya está en uso.", "Nick");
                }
                else if (ex.Message.Contains("UQ__Usuario__A9D105344761A97B"))
                {
                    throw new UniqueKeyViolationException("El correo electrónico ya está en uso.", "Email");
                }
                throw new UniqueKeyViolationException("Infracción de la restricción UNIQUE KEY.", "Unknown");
            }
            catch (Exception ex)
            {
                if (ex.InnerException!.Message.Contains("UQ__Usuario__7D3471B6867373C0"))
                {
                    throw new UniqueKeyViolationException("El nick ya está en uso.", "Nick");
                }
                else if (ex.InnerException.Message.Contains("UQ__Usuario__A9D105344761A97B"))
                {
                    throw new UniqueKeyViolationException("El correo electrónico ya está en uso.", "Email");
                }
                throw new Exception("Ocurrio un problema en el servidor.");
            }
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