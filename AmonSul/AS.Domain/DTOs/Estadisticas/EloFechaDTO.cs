namespace AS.Domain.DTOs.Estadisticas;

/// <summary>
/// Una fila del historial de ELO: para saber qué ELO tenía cada jugador el día de una partida
/// </summary>
public class EloFechaDTO
{
    public int IdElo { get; set; }
    public int IdUsuario { get; set; }
    public int Elo { get; set; }
    public DateTime Fecha { get; set; }
}
