namespace AS.Application.DTOs.Usuario;

public class CambiarPassDTO
{
    public int idUsuario { get; set; }
    public string? OldPass { get; set; }
    public string? NewPass { get; set; }
}
