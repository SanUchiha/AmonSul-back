namespace AS.Application.DTOs.Elo;

public class CreateEloDTO
{
    public required int IdUsuario { get; set; }
    public required int PuntuacionElo { get; set; }
    public DateTime FechaElo { get; set; } = DateTime.Now;
}
