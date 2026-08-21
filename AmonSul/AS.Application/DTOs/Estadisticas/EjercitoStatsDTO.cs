namespace AS.Application.DTOs.Estadisticas;

public class EjercitoStatsDTO
{
    public string Ejercito { get; set; } = string.Empty;
    public string? Bando { get; set; }
    public int TotalPartidas { get; set; }
    public int Victorias { get; set; }
    public int Derrotas { get; set; }
    public int Empates { get; set; }
    public double WinRate { get; set; }
    public double MediaPuntosFavor { get; set; }
    public double MediaPuntosContra { get; set; }
}
