using AS.Domain.DTOs.Equipo;

namespace AS.Application.DTOs.PartidaTorneo;

public class GenerarOtraRondaEquiposRequestDTO
{
    public int IdTorneo { get; set; }
    public int IdRonda { get; set; }
    public List<EquipoDTO> Clasificacion { get; set; } = [];
    public bool PermiteRepetirRival { get; set; }
    public bool NecesitaBye { get; set; }
    public List<PairingDTO> PairingRondaAnterior { get; set; } = [];
}
