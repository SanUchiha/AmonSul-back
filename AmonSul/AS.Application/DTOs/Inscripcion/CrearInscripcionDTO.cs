namespace AS.Application.DTOs.Inscripcion;

public class CrearInscripcionDTO
{
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public string? EstadoInscripcion { get; set; } = "EN PROCESO";
    public DateOnly? FechaInscripcion { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string? EstadoLista { get; set; } = "NO ENTREGADA";
    public string EsPago { get; set; } = "NO";
}
