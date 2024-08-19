using System.ComponentModel.DataAnnotations;

namespace AS.Application.DTOs.Inscripcion;

public class ActualizarEstadoInscripcion
{
    [Required]
    public int IdInscripcion { get; set; }
    [Required]
    public string? EstadoInscripcion { get; set; }
}
