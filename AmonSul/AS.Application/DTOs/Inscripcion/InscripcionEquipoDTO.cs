namespace AS.Application.DTOs.Inscripcion;

public class InscripcionEquipoDTO
{
    public int IdInscripcion { get; set; }
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public int IdCapitan { get; set; }
    public int IdEquipo { get; set; }
    public int IdOrganizador { get; set; }
    public required string NombreEquipo { get; set; }
    public DateOnly? FechaInscripcion { get; set; }
    public string? EsPago { get; set; }
    // El de esta inscripción: si su jugador deja que el organizador vea su correo
    public bool? CompartirEmail { get; set; }
    public List<ComponentesEquipoDTO> ComponentesEquipoDTO { get; set; } = [];
}
