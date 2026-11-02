namespace AS.Application.DTOs.Inscripcion;

public class CreateEquipoDTO
{
    public required string NombreEquipo { get; set; }
    public int IdCapitan { get; set; }
    public List<int>? Miembros { get; set; } = [];
    public int IdTorneo { get; set; }
    // Si el capitán deja que el organizador vea su correo. Los demás miembros lo deciden cada uno en su inscripción
    public bool? CompartirEmail { get; set; }
}
