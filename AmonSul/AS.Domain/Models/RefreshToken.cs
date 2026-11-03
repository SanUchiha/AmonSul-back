namespace AS.Domain.Models;

// La sesión que dura: con él la app pide un JWT nuevo cuando caduca el de 12 horas, sin volver a iniciar sesión.
// En la BD solo su hash, como el enlace de recuperar la contraseña
public class RefreshToken
{
    public int IdRefreshToken { get; set; }
    public int IdUsuario { get; set; }
    public required string Hash { get; set; }
    public DateTime Caducidad { get; set; }
    public DateTime FechaCreacion { get; set; }
}
