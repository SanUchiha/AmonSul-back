namespace AS.Application.DTOs.PartidaAmistosa;

public class ViewUsuarioPartidaDTO
{
    public int IdUsuario { get; set; }
    public string Email { get; set; } = null!;
    public string Nick { get; set; } = null!;
    public string? Ciudad { get; set; }
    public DateOnly FechaRegistro { get; set; }
    public int? IdFaccion { get; set; }
    public int NumeroPartidasJugadas { get; set; }
    public int PartidasGanadas { get; set; }
    public int PartidasEmpatadas { get; set; }
    public int PartidasPerdidas { get; set; }
    public int PuntuacionElo { get; set; }
    public int ClasificacionElo { get; set; }
}
