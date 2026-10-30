using AS.Domain.DTOs.Torneo;

namespace AS.Application.DTOs.PartidaTorneo;

public class EstaJugandoDTO
{
    public bool TienePartidas { get; set; }
    public List<PartidaTorneoDTO> Partidas { get; set; } = [];
    public required string TorneoType { get; set; }
    // Solo en Parejas: una lista por pareja en vez de una por jugador
    public bool ListaPorPareja { get; set; }
    // En una liga la partida se nombra por su jornada, no por la mesa
    public string? FormatoTorneo { get; set; }
}