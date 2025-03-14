namespace AS.Application.DTOs.Inscripcion;

public class CreateEquipoDTO
{
    public required string NombreEquipo { get; set; }
    public int IdCapitan { get; set; }
    public List<int>? Miembros { get; set; } = [];
    public int IdTorneo { get; set; }
}
