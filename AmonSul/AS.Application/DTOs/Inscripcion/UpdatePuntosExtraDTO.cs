using System.ComponentModel.DataAnnotations;

namespace AS.Application.DTOs.Inscripcion;

public class UpdatePuntosExtraDTO
{
    [Required]
    public int IdInscripcion { get; set; }
    [Required]
    public int PuntosExtra { get; set; }
}
