using System.Security.Cryptography;
using System.Text;

namespace AS.Infrastructure;

/// <summary>
/// Contraseñas con BCrypt. Hay tres formatos guardados:
/// - BCrypt de la contraseña: el de ahora.
/// - SHA-256 sin sal (64 caracteres hex): el antiguo.
/// - El SHA-256 antiguo envuelto en BCrypt, con el prefijo «sha256+»: así quedan protegidas las de quien no entra.
/// Las dos antiguas se aceptan y, al entrar, se cambian por BCrypt de la contraseña
/// </summary>
public static class Contrasenas
{
    // Unos 100-200 ms por comprobación: lento para quien prueba contraseñas, imperceptible al entrar
    private const int COSTE = 11;
    private const string PREFIJO_SHA_ENVUELTO = "sha256+";

    // Para comparar aunque el usuario no exista: que el tiempo de respuesta no diga qué correos están dados de alta
    private static readonly string HASH_DE_RELLENO = BCrypt.Net.BCrypt.HashPassword("relleno", COSTE);

    public static string Cifrar(string contrasena) => BCrypt.Net.BCrypt.HashPassword(contrasena, COSTE);

    public static bool Comprobar(string contrasena, string? guardada)
    {
        if (string.IsNullOrEmpty(guardada))
        {
            BCrypt.Net.BCrypt.Verify(contrasena, HASH_DE_RELLENO);
            return false;
        }

        try
        {
            if (guardada.StartsWith(PREFIJO_SHA_ENVUELTO, StringComparison.Ordinal))
                return BCrypt.Net.BCrypt.Verify(Sha256(contrasena), guardada[PREFIJO_SHA_ENVUELTO.Length..]);

            if (guardada.StartsWith("$2", StringComparison.Ordinal))
                return BCrypt.Net.BCrypt.Verify(contrasena, guardada);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }

        // SHA-256 sin sal: comparación de tiempo constante
        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(Sha256(contrasena)),
            Encoding.ASCII.GetBytes(guardada.ToLowerInvariant()));
    }

    // Si no es BCrypt de la contraseña, al entrar se guarda de nuevo ya en BCrypt
    public static bool HayQueActualizar(string? guardada) =>
        string.IsNullOrEmpty(guardada) || !guardada.StartsWith("$2", StringComparison.Ordinal);

    // Para envolver de una vez los SHA-256 antiguos que aún quedan
    public static string EnvolverSha256(string sha256Hex) =>
        PREFIJO_SHA_ENVUELTO + BCrypt.Net.BCrypt.HashPassword(sha256Hex.ToLowerInvariant(), COSTE);

    public static bool EsSha256SinEnvolver(string? guardada) =>
        guardada is { Length: 64 } && guardada.All(Uri.IsHexDigit);

    private static string Sha256(string texto) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto))).ToLowerInvariant();
}
