using AS.Application.DTOs.Faccion;

namespace AS.Application.DTOs.Usuario;

public class UsuarioDTO
{
    public int IdUsuario { get; set; }
    public string? Nick { get; set; }
    public int? IdFaccion { get; set; } 
    public string? Ciudad { get; set; }
    public FaccionDTO? Faccion { get; set; }
}
