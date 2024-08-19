using System.ComponentModel.DataAnnotations;

namespace AS.Application.DTOs.Inscripcion;

public class ActualizarEstadoPago
{
    [Required]
    public int IdInscripcion { get; set; }
    [Required]
    public string? EsPago { get; set; }
}
