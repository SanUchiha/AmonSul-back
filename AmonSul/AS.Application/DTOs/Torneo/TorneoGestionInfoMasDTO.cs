using AS.Application.DTOs.Inscripcion;

namespace AS.Application.DTOs.Torneo;

public class TorneoGestionInfoMasDTO
{
    public TorneoCreadoDTO? Torneo { get; set; }
    public List<InscripcionTorneoCreadoMasDTO>? Inscripciones { get; set; }
}
