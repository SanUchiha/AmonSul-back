using AS.Domain.Models;
using AS.Infrastructure.DTOs;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly DbamonsulContext _dbamonsulContext;
        private readonly Utilidades _utilidades;

        public AccountRepository(DbamonsulContext dbamonsulContext, Utilidades utilidades)
        {
            _dbamonsulContext = dbamonsulContext;
            _utilidades = utilidades;
        }

        public async Task<LoginResponse> Login(LoginDTO loginDTO)
        {
            var foundUser = await _dbamonsulContext.Usuarios.Where(u =>
                u.Email == loginDTO.Email &&
                u.Contraseña == _utilidades.encriptarSHA256(loginDTO.Password!))
                .FirstOrDefaultAsync();

            if (foundUser == null) return new LoginResponse { IsAccess = false };

            return new LoginResponse
            {
                IsAccess = false,
                Token = _utilidades.generarJWT(foundUser)
            };

        }        
    }
}
