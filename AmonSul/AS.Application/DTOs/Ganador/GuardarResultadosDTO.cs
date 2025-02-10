using AS.Application.DTOs.PartidaTorneo;

namespace AS.Application.DTOs.Ganador;

public class GuardarResultadosDTO
{
    public required List<GanadorDTO> GanadoresDTO { get; set; }
    public required GenerarRondaDTO GenerarRondaDTO { get; set; }
}
