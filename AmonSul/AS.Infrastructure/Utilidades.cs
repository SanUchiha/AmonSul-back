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
