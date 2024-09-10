namespace AS.Application.DTOs.PartidaTorneo;

public class GenerarRondaDTO
{
    public List<EmparejamientoDTO>? Emparejamientos { get; set; }
    public bool EsEloCheck { get; set; }
    public bool LuzVsOscCheck { get; set; }
    public bool MismaComunidadCheck { get; set; }
    public string? OpcionImpares { get; set; }
    public bool RetosCheck { get; set; }
    public int IdRonda { get; set; }
    public int IdTorneo { get; set; }
}
