namespace AS.Application.DTOs.Inscripcion;

public class CrearInscripcionDTO
{
    public int? IdTorneo { get; set; }
    public int? IdUsuario { get; set; }
    public string? EstadoInscripcion { get; set; }
    public DateOnly? FechaInscripcion { get; set; }  = DateOnly.FromDateTime(DateTime.Now);
}
