namespace AS.Application.DTOs.Estadisticas;

public class TopBandosResponseDTO
{
    public BandoTopDTO Bien { get; set; } = new();
    public BandoTopDTO Oscuridad { get; set; } = new();
}

public class BandoTopDTO
{
    public List<EjercitoStatsDTO> Mejores { get; set; } = [];
    public List<EjercitoStatsDTO> Peores { get; set; } = [];
}
