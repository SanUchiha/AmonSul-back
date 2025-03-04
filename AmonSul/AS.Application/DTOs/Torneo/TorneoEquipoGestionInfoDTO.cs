using AS.Domain.DTOs.Equipo;

namespace AS.Application.DTOs.Torneo;

public class TorneoEquipoGestionInfoDTO
{
    public required TorneoCreadoDTO Torneo { get; set; }
    public List<EquipoDTO> Equipos { get; set; } = [];
}
