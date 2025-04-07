using AS.Application.DTOs.Lista;

namespace AS.Application.DTOs.Inscripcion;

public class InscripcionTorneoCreadoMasDTO
{
    public int IdInscripcion { get; set; }
    public int IdUsuario { get; set; }
    public int IdTorneo { get; set; }
    public string? Nick { get; set; }
    public DateOnly? FechaInscripcion { get; set; }
    public DateOnly? FechaUltimaEntrega { get; set; }
    public string? EsPago { get; set; }
    public List<ListaViewDTO> Lista { get; set; } = [];
    public int CountListasEntregadas { get; set; } = 0;
}
