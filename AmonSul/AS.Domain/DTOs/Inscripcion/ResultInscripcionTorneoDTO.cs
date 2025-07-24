namespace AS.Domain.DTOs.Inscripcion;

public class ResultInscripcionTorneoDTO
{
    public bool Result { get; set; }
    public string? Mensaje { get; set; } = string.Empty;
    public int? IdInscripcion { get; set; }
    public int PuntosExtra { get; set; }
}
