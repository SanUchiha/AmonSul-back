using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure;
using AS.Infrastructure.DTOs.Login;
using Microsoft.EntityFrameworkCore;

namespace AS.Application.Services;

public class LoginApplication : ILoginApplication
{
    private readonly DbamonsulContext _dbamonsulContext;
    private readonly Utilidades _utilidades;

    public LoginApplication(DbamonsulContext dbamonsulContext, Utilidades utilidades)
    {
        _dbamonsulContext = dbamonsulContext;
        _utilidades = utilidades;
    }

    public async Task<LoginResponse> Login(LoginDTO loginDTO)
    {
        var foundUser = await _dbamonsulContext.Usuarios.Where(u =>
            u.Email == loginDTO.Email &&
            u.Contraseña == _utilidades.encriptarSHA256(loginDTO.Password!)).
            FirstOrDefaultAsync();

        if (foundUser == null) return new LoginResponse { IsAccess = false };

        var token = _utilidades.generarJWT(foundUser);

        var loginResponse = new LoginResponse
        {
            IsAccess = true,
            User = loginDTO.Email,
            Token = token, 
            IdUsuario = foundUser.IdUsuario.ToString(),
        };

        return loginResponse;
    }
}
