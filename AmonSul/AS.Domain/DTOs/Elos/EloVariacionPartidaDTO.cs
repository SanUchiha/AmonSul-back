namespace AS.Domain.DTOs.Elos;

/// <summary>
/// ELO ganado o perdido por un jugador en una partida de torneo
/// </summary>
public class EloVariacionPartidaDTO
{
    public int IdPartidaTorneo { get; set; }
    public int IdUsuario { get; set; }
    public int Variacion { get; set; }
}
