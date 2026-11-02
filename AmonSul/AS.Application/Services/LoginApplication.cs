using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure;
using AS.Infrastructure.Data;
using AS.Infrastructure.DTOs.Login;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AS.Application.Services;

public class LoginApplication(
    DbamonsulContext dbamonsulContext, 
    Utilidades utilidades, 
    IEloApplication eloApplication,
    IMemoryCache cache) 
        : ILoginApplication
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;
    private readonly Utilidades _utilidades = utilidades;
    private readonly IEloApplication _eloApplication = eloApplication;
    private readonly IMemoryCache _cache = cache;

    // Tras 5 fallos seguidos con un correo, ese correo no puede entrar en 15 minutos: frena a quien prueba contraseñas
    private const int MAXIMO_FALLOS = 5;
    private static readonly TimeSpan BLOQUEO = TimeSpan.FromMinutes(15);

    public static string ClaveFallos(string email) => $"login-fallos:{email.Trim().ToLowerInvariant()}";

    public async Task<LoginResponse> Login(LoginDTO loginDTO)
    {
        if (string.IsNullOrWhiteSpace(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
            return new LoginResponse { IsAccess = false };

        string clave = ClaveFallos(loginDTO.Email);
        if (_cache.TryGetValue(clave, out int fallos) && fallos >= MAXIMO_FALLOS)
            return new LoginResponse
            {
                IsAccess = false,
                Mensaje = "Demasiados intentos fallidos. Espera 15 minutos y vuelve a probar.",
            };

        // Solo lo necesario: la foto de perfil, que en algunos pesa megas, no hace falta para entrar
        var cuenta = await _dbamonsulContext.Usuarios
            .Where(u => u.Email == loginDTO.Email)
            .Select(u => new { u.IdUsuario, u.Email, u.Contraseña })
            .FirstOrDefaultAsync();

        // Se comprueba aunque el correo no exista, para que tarde lo mismo en los dos casos
        if (!Contrasenas.Comprobar(loginDTO.Password, cuenta?.Contraseña) || cuenta is null)
        {
            _cache.Set(clave, fallos + 1, BLOQUEO);
            return new LoginResponse { IsAccess = false };
        }
        _cache.Remove(clave);

        // Las guardadas en el formato antiguo pasan a BCrypt al entrar
        if (Contrasenas.HayQueActualizar(cuenta.Contraseña))
        {
            string nueva = Contrasenas.Cifrar(loginDTO.Password);
            await _dbamonsulContext.Usuarios
                .Where(u => u.IdUsuario == cuenta.IdUsuario)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Contraseña, nueva));
        }

        // Una vez más y la fecha: de una sola vez en la base de datos, aunque entre desde dos sitios a la vez
        await _dbamonsulContext.Usuarios
            .Where(u => u.IdUsuario == cuenta.IdUsuario)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.NumeroLogins, u => u.NumeroLogins + 1)
                .SetProperty(u => u.UltimoLogin, DateTime.Now));

        var token = _utilidades.GenerarJWT(new Usuario { IdUsuario = cuenta.IdUsuario, Email = cuenta.Email });

        var loginResponse = new LoginResponse
        {
            IsAccess = true,
            User = loginDTO.Email,
            Token = token,
            IdUsuario = cuenta.IdUsuario.ToString(),
        };

        await InitElo(cuenta.IdUsuario);

        return loginResponse;
    }

    private async Task InitElo(int idUsuario) => 
        await _eloApplication.CheckEloByUser(idUsuario);
}
