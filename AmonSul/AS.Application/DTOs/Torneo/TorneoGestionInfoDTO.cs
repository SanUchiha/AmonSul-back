using AS.Application.DTOs.Inscripcion;

namespace AS.Application.DTOs.Torneo;

public class TorneoGestionInfoDTO
{
    public TorneoCreadoDTO? Torneo { get; set; }
    public List<InscripcionTorneoCreadoDTO>? Inscripciones { get; set; }
}
