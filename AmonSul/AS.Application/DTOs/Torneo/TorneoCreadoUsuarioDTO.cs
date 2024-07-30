using AS.Application.DTOs.Inscripcion;

namespace AS.Application.DTOs.Torneo;

public class TorneoCreadoUsuarioDTO
{
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public string NombreTorneo { get; set; } = null!;
    public List<InscripcionTorneoCreadoDTO> InscripcionTorneos { get; set; } = [];
}
