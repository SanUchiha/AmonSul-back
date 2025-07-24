namespace AS.Application.DTOs.PartidaAmistosa;

public class CreatePartidaAmistosaDTO
{
    public required int IdUsuario1 { get; set; }
    public required int IdUsuario2 { get; set; }
    public required int ResultadoUsuario1 { get; set; }
    public required int ResultadoUsuario2 { get; set; }
    public DateOnly FechaPartida { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public required bool EsMatchedPlayPartida { get; set; }
    public string? EscenarioPartida { get; set; } = string.Empty;
    public bool EsElo { get; set; } 
    public required int PuntosPartida { get; set; }
    public bool EsTorneo{ get; set; }
    public string? EjercitoUsuario1 { get; set; } = string.Empty;
    public string? EjercitoUsuario2 { get; set; } = string.Empty;
    public bool? PartidaValidadaUsuario1 { get; set; }
}
