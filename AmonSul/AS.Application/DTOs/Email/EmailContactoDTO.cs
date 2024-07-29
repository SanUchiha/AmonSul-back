using System.ComponentModel.DataAnnotations;

namespace AS.Application.DTOs.Email;

public class EmailContactoDTO
{
    [Required]
    public string? Email{ get; set; }
    [Required]
    public string? Message { get; set; }
}
