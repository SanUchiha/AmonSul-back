namespace AS.Application.DTOs.PartidaTorneo;

public class UpdatePartidaTorneoDTO
{
    public int IdPartidaTorneo { get; set; }
    public int? ResultadoUsuario1 { get; set; }
    public int? ResultadoUsuario2 { get; set; }
    public string? EscenarioPartida { get; set; }
    public int? GanadorPartidaTorneo { get; set; }
    public bool? PartidaValidadaUsuario1 { get; set; }
    public bool? PartidaValidadaUsuario2 { get; set; }
    public bool? LiderMuertoUsuario1 { get; set; }
    public bool? LiderMuertoUsuario2 { get; set; }
    public DateOnly? FechaPartida { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string? EjercitoUsuario1 { get; set; }
    public string? EjercitoUsuario2 { get; set; }
}
