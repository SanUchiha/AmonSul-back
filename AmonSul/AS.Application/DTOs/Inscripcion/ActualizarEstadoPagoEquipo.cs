using System.ComponentModel.DataAnnotations;

namespace AS.Application.DTOs.Inscripcion;

public class ActualizarEstadoPagoEquipo
{
    [Required]
    public int IdEquipo { get; set; }
    [Required]
    public string? EsPago { get; set; }
}
