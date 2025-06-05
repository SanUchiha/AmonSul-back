namespace AS.Application.DTOs.Torneo;

public class EquipoEmparejamientoDTO
{
    public string? NombreEquipo { get; set; }
    public int IdEquipo { get; set; }
    public int IdCapitan { get; set; }
    public string? NickCapitan { get; set; }
    public List<InscripcionTorneoEmparejamientoDTO> Inscripciones { get; set; } = [];
}