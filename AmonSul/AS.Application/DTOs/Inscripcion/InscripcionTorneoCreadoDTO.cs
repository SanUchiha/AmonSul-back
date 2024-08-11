namespace AS.Application.DTOs.Inscripcion;

public class InscripcionTorneoCreadoDTO
{
    public int IdInscripcion { get; set; }
    public int? IdUsuario { get; set; }
    public string? Nick { get; set; }
    public string? EstadoInscripcion { get; set; }
    public DateOnly? FechaInscripcion { get; set; }
    public string? ListaData { get; set; }
    public string? EstadoLista { get; set; }
    public DateOnly? FechaEntrega { get; set; }
    public bool? EsPago { get; set; }
}
