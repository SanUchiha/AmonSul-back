namespace AS.Application.DTOs.Inscripcion;

public class InscripcionUsuarioEquipoDTO
{
    public int IdInscripcion { get; set; }
    public int IdTorneo { get; set; }
    public required string NombreTorneo { get; set; }
    public int IdUsuario { get; set; }
    public required string Nick { get; set; }
    public int IdEquipo { get; set; }
}
