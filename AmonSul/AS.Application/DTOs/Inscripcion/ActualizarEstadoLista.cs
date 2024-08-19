using System.ComponentModel.DataAnnotations;

namespace AS.Application.DTOs.Inscripcion;

public class ActualizarEstadoLista
{
    [Required]
    public int IdInscripcion { get; set; }
    [Required]
    public string? EstadoLista { get; set; }
}
