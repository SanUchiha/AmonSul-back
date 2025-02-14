using AS.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AS.Infrastructure;

public class Utilidades(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public static string EncriptarSHA256(string input)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        StringBuilder stringBuilder = new();
        for (int i = 0; i < bytes.Length; i++)
        {
            stringBuilder.Append(bytes[i].ToString("x2"));
        }

        return stringBuilder.ToString();
    }

    public static string GenerarPassTemporal()
    {
        const string caracteresPermitidos = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const int longitud = 10;
        Random random = new();
        return new string(Enumerable.Repeat(caracteresPermitidos, longitud)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public string GenerarJWT(Usuario usuario) 
    {
        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),

        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        //Crear detalle del token
        var jwtConfig = new JwtSecurityToken
            (
                claims: userClaims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: credentials

            );

        return new JwtSecurityTokenHandler().WriteToken(jwtConfig);
    }
}
