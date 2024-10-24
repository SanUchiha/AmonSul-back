namespace AS.Application.DTOs.Partida;

public class MatchDTO
{
    public int IdUsuario1 { get; set; }
    public string? NickUsuario1 { get; set; }
    public int IdUsuario2 { get; set; }
    public string? NickUsuario2 { get; set; }
    public int ResultadoUsuario1 { get; set; }
    public int ResultadoUsuario2 { get; set; }
    public DateOnly? FechaPartida { get; set; }
    public string? EscenarioPartida { get; set; }
    public int PuntosPartida { get; set; }
    public string? GanadorPartidaNick { get; set; }
    public int GanadorPartida { get; set; }
    public bool EsTorneo { get; set; }
    public string? EjercitoUsuario1 { get; set; }
    public string? EjercitoUsuario2 { get; set; }
}
