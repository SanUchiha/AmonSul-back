namespace AS.Application.DTOs.Torneo;

public class GenerarRondaEquiposDTO
{
    public int IdTorneo { get; set; }
    public int IdRonda { get; set; }
    public bool IsTorneoNarsil { get; set; }
    public bool NecesitaBye { get; set; }
    public List<EmparejamientoEquiposDTO> EmparejamientosEquipos { get; set; } = [];
}
