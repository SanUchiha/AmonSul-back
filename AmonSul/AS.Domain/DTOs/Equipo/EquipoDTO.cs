using AS.Domain.DTOs.Inscripcion;

namespace AS.Domain.DTOs.Equipo;

public class EquipoDTO
{
    public string? NombreEquipo { get; set; }
    public int IdEquipo { get; set; }
    public int IdCapitan { get; set; }
    public string? NickCapitan { get; set; }
    public string? EmailCapitan { get; set; }
    public string? EsPago { get; set; }
    public DateOnly? FechaInscripcion { get; set; }
    public List<InscripcionTorneoDTO> Inscripciones { get; set; } = [];
}
