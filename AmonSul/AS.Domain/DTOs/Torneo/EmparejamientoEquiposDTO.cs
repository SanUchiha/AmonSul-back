namespace AS.Application.DTOs.Torneo;

public class EmparejamientoEquiposDTO
{
    public required EquipoEmparejamientoDTO Equipo1 { get; set; }
    public required EquipoEmparejamientoDTO Equipo2 { get; set; }
}