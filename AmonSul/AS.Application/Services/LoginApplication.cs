using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure;
using AS.Infrastructure.Data;
using AS.Infrastructure.DTOs.Login;
using Microsoft.EntityFrameworkCore;

namespace AS.Application.Services;

public class LoginApplication(
    DbamonsulContext dbamonsulContext, 
    Utilidades utilidades, 
    IEloApplication eloApplication) 
        : ILoginApplication
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;
    private readonly Utilidades _utilidades = utilidades;
    private readonly IEloApplication _eloApplication = eloApplication;

    public async Task<LoginResponse> Login(LoginDTO loginDTO)
    {
        Usuario? foundUser = await _dbamonsulContext.Usuarios.Where(u =>
            u.Email == loginDTO.Email &&
            u.Contraseña == Utilidades.EncriptarSHA256(loginDTO.Password!)).
            FirstOrDefaultAsync();

        if (foundUser == null) return new LoginResponse { IsAccess = false };

        var token = _utilidades.GenerarJWT(foundUser);

        var loginResponse = new LoginResponse
        {
            IsAccess = true,
            User = loginDTO.Email,
            Token = token,
            IdUsuario = foundUser.IdUsuario.ToString(),
        };

        await InitElo(foundUser.IdUsuario);

        return loginResponse;
    }

    private async Task InitElo(int idUsuario) => 
        await _eloApplication.CheckEloByUser(idUsuario);
}
