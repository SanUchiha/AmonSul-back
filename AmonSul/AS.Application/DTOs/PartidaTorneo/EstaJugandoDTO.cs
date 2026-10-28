using AS.Domain.DTOs.Torneo;

namespace AS.Application.DTOs.PartidaTorneo;

public class EstaJugandoDTO
{
    public bool TienePartidas { get; set; }
    public List<PartidaTorneoDTO> Partidas { get; set; } = [];
    public required string TorneoType { get; set; }
    // Solo en Parejas: una lista por pareja en vez de una por jugador
    public bool ListaPorPareja { get; set; }
}