namespace AS.Application.DTOs.Elo;

public class EloUsuarioDTO
{
    public int IdUsuario { get; set; }
    public required string Nick { get; set; }
    public int PuntuacionElo { get; set; }
    public DateTime FechaElo { get; set; }
}
