using AS.Domain.DTOs.Torneo;

namespace AS.Domain.DTOs.Inscripcion;

public class InscripcionTorneoDTO
{
    public int IdInscripcion { get; set; }
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public DateOnly? FechaInscripcion { get; set; }
    public string? EstadoLista { get; set; }
    public DateOnly? FechaEntregaLista { get; set; }
    public string? EsPago { get; set; }
    public string? ListaData { get; set; }
    public string? Ejercito { get; set; }
    public int IdLista { get; set; }
    public string? Nick { get; set; }
    public TorneoViewDTO? Torneo { get; set; }
}
