using AS.Application.DTOs.Faccion;
using AS.Domain.DTOs.Inscripcion;

namespace AS.Application.DTOs.Usuario;

public class UsuarioInscripcionTorneoDTO
{
    public int IdUsuario { get; set; }
    public string? Nick { get; set; }
    public int? IdFaccion { get; set; }
    public string? Ciudad { get; set; }
    public FaccionDTO? Faccion { get; set; }
    public List<InscripcionDTO> InscripcionTorneos { get; set; } = [];
}
