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

    // La sesión dura 30 días desde la última vez que se usa: el refresh token se cambia por uno nuevo en cada uso.
    // El usado aún vale un minuto: si dos pestañas lo usan a la vez, ninguna se queda fuera
    private static readonly TimeSpan VALIDEZ_REFRESH_TOKEN = TimeSpan.FromDays(30);
    private static readonly TimeSpan MARGEN_REFRESH_TOKEN_USADO = TimeSpan.FromMinutes(1);

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
            RefreshToken = await NuevoRefreshTokenAsync(cuenta.IdUsuario),
        };

        await InitElo(cuenta.IdUsuario);

        return loginResponse;
    }

    // Un token nuevo sin la contraseña. No cuenta como inicio de sesión: Numero_Logins y Ultimo_Login no cambian
    public async Task<LoginResponse> Refresh(string refreshToken)
    {
        string hash = Contrasenas.HashDeRefreshToken(refreshToken);
        DateTime ahora = DateTime.UtcNow;

        var guardado = await _dbamonsulContext.RefreshTokens
            .Where(r => r.Hash == hash && r.Caducidad > ahora)
            .Select(r => new { r.IdRefreshToken, r.IdUsuario, r.Caducidad })
            .FirstOrDefaultAsync();
        if (guardado is null) return new LoginResponse { IsAccess = false };

        DateTime finDelMargen = ahora + MARGEN_REFRESH_TOKEN_USADO;
        if (guardado.Caducidad > finDelMargen)
            await _dbamonsulContext.RefreshTokens
                .Where(r => r.IdRefreshToken == guardado.IdRefreshToken)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Caducidad, finDelMargen));

        var cuenta = await _dbamonsulContext.Usuarios
            .Where(u => u.IdUsuario == guardado.IdUsuario)
            .Select(u => new { u.IdUsuario, u.Email })
            .FirstOrDefaultAsync();
        if (cuenta is null) return new LoginResponse { IsAccess = false };

        return new LoginResponse
        {
            IsAccess = true,
            User = cuenta.Email,
            Token = _utilidades.GenerarJWT(new Usuario { IdUsuario = cuenta.IdUsuario, Email = cuenta.Email }),
            IdUsuario = cuenta.IdUsuario.ToString(),
            RefreshToken = await NuevoRefreshTokenAsync(cuenta.IdUsuario),
        };
    }

    // Sin esto, un token copiado seguiría abriendo la sesión hasta caducar, aunque el móvil ya lo haya borrado
    public async Task CerrarSesion(string refreshToken)
    {
        string hash = Contrasenas.HashDeRefreshToken(refreshToken);

        await _dbamonsulContext.RefreshTokens
            .Where(r => r.Hash == hash)
            .ExecuteDeleteAsync();
    }

    private async Task<string> NuevoRefreshTokenAsync(int idUsuario)
    {
        var (token, hash) = Contrasenas.NuevoRefreshToken();
        DateTime ahora = DateTime.UtcNow;

        // Los caducados de la cuenta ya no sirven: se limpian al crear uno
        await _dbamonsulContext.RefreshTokens
            .Where(r => r.IdUsuario == idUsuario && r.Caducidad <= ahora)
            .ExecuteDeleteAsync();

        _dbamonsulContext.RefreshTokens.Add(new RefreshToken
        {
            IdUsuario = idUsuario,
            Hash = hash,
            Caducidad = ahora + VALIDEZ_REFRESH_TOKEN,
            FechaCreacion = ahora,
        });
        await _dbamonsulContext.SaveChangesAsync();

        return token;
    }

    private async Task InitElo(int idUsuario) => 
        await _eloApplication.CheckEloByUser(idUsuario);
}
