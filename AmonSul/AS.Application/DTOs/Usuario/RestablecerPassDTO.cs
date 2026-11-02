namespace AS.Application.DTOs.Usuario;

public class RestablecerPassDTO
{
    // El que llega en el enlace del correo
    public string? Token { get; set; }
    public string? NuevaPass { get; set; }
}
