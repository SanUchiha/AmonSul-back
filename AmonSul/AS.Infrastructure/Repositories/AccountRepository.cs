using AS.Infrastructure.Data;
using AS.Infrastructure.DTOs.Login;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

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
        // Solo lo necesario: la foto de perfil, que en algunos pesa megas, no hace falta para entrar
        var cuenta = await _dbamonsulContext.Usuarios
            .Where(u => u.Email == loginDTO.Email)
            .Select(u => new { u.IdUsuario, u.Email, u.Contraseña })
            .FirstOrDefaultAsync();

        if (cuenta is null || !Contrasenas.Comprobar(loginDTO.Password!, cuenta.Contraseña))
            return new LoginResponse { IsAccess = false };

        var token = _utilidades.GenerarJWT(new Domain.Models.Usuario { IdUsuario = cuenta.IdUsuario, Email = cuenta.Email });

        var loginResponse = new LoginResponse
        {
            IsAccess = true,
            Token = token
        };

        return loginResponse; 

    }        
}
