using AS.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AS.Infrastructure
{
    public class Utilidades
    {
        private readonly IConfiguration _configuration;

        public Utilidades(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string encriptarSHA256(string input) 
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    stringBuilder.Append(bytes[i].ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }

        public string generarJWT(Usuario usuario) 
        {
            //Crear la info del user para el token
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
                    expires: DateTime.UtcNow.AddMinutes(60),
                    signingCredentials: credentials

                );

            return new JwtSecurityTokenHandler().WriteToken(jwtConfig);
        }

    }
}
