namespace AS.Application.DTOs.Torneo;

public class TorneoCreadoUsuarioDTO
{
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public string NombreTorneo { get; set; } = null!;
    public string EstadoTorneo { get; set; } = null!;
}
