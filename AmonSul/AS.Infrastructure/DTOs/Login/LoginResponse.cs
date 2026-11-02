namespace AS.Infrastructure.DTOs.Login;

public sealed class LoginResponse
{
    public bool IsAccess { get; set; }
    public string? User { get; set; }
    public string? Token { get; set; }
    public string? IdUsuario { get; set; }
    // Por qué no entra, si no es usuario o contraseña incorrectos: por ejemplo, demasiados intentos
    public string? Mensaje { get; set; }
}