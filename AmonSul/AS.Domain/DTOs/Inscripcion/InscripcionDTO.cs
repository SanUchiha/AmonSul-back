using AS.Domain.DTOs.Lista;

namespace AS.Domain.DTOs.Inscripcion;

public class InscripcionDTO
{
    public int IdInscripcion { get; set; }
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public int? IdEquipo { get; set; }

    public List<ListaDTO> Lista { get; set; } = [];
}
