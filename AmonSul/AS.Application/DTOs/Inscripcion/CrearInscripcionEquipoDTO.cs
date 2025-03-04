namespace AS.Application.DTOs.Inscripcion;

public class CrearInscripcionEquipoDTO
{
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public DateOnly? FechaInscripcion { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string? EstadoLista { get; set; } = "NO ENTREGADA";
    public string EsPago { get; set; } = "NO";
    public int IdEquipo { get; set; }
}
