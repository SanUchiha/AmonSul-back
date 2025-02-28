namespace AS.Application.DTOs.Inscripcion;

public class InscripcionTorneoEquiposDTO
{
    public int IdInscripcion { get; set; }
    public int IdTorneo { get; set; }
    public int IdCapitan { get; set; }
    public DateOnly? FechaInscripcion { get; set; }
    public string? EsPago { get; set; }
    public List<int> IdsListas { get; set; } = [];
    public List<ComponentesEquipoDTO> ComponentesEquipo { get; set; } = [];
    public int IdEquipo { get; set; }
    public string? NombreEquipo { get; set; }
}
