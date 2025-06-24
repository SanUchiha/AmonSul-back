namespace AS.Application.DTOs.Inscripcion;

public class JugadoresEquipoParaCambioDTO
{
    public int IdEquipo { get; set; }
    public string? NombreEquipo { get; set; }
    public List<ComponentesEquipoDTO> ComponentesEquipoDTO { get; set; } = [];
}
