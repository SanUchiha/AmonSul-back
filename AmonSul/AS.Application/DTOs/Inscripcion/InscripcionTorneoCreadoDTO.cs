namespace AS.Application.DTOs.Inscripcion;

public class InscripcionTorneoCreadoDTO
{
    public int IdInscripcion { get; set; }
    public int IdUsuario { get; set; }
    public int IdTorneo { get; set; }
    public int IdLista { get; set; }
    public int IdEquipo { get; set; }
    public string? Nick { get; set; }
    public DateOnly? FechaInscripcion { get; set; }
    public string? Ejercito { get; set; }
    public string? EstadoLista { get; set; }
    public DateOnly? FechaEntrega { get; set; }
    public string? EsPago { get; set; }
    public string? Bando { get; set; }
    public int PuntosExtra { get; set; }
}
