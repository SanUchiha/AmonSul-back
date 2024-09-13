namespace AS.Application.DTOs.PartidaTorneo;

public class PartidaTorneoDTO
{
    public int IdPartidaTorneo { get; set; }
    public int? IdTorneo { get; set; }
    public int? IdUsuario1 { get; set; }
    public int? IdUsuario2 { get; set; }
    public int? ResultadoUsuario1 { get; set; }
    public int? ResultadoUsuario2 { get; set; }
    public DateOnly? FechaPartida { get; set; }
    public string? EscenarioPartida { get; set; }
    public int? GanadorPartidaTorneo { get; set; }
    public bool? PartidaValidadaUsuario1 { get; set; }
    public bool? PartidaValidadaUsuario2 { get; set; }
    public string? EjercitoUsuario1 { get; set; }
    public string? EjercitoUsuario2 { get; set; }
    public int? NumeroRonda { get; set; }
    public bool? LiderMuertoUsuario1 { get; set; }
    public bool? LiderMuertoUsuario2 { get; set; }
    public string? Nick1 { get; set; }
    public string? Nick2 { get; set; }
}
