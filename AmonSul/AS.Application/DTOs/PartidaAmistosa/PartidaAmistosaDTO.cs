namespace AS.Application.DTOs.PartidaAmistosa;

public class PartidaAmistosaDTO
{
    public int IdPartidaAmistosa { get; set; }
    public int? IdUsuario1 { get; set; }
    public int? IdUsuario2 { get; set; }
    public int? ResultadoUsuario1 { get; set; }
    public int? ResultadoUsuario2 { get; set; }
    public DateOnly? FechaPartida { get; set; }
    public bool? EsMatchedPlayPartida { get; set; }
    public string? EscenarioPartida { get; set; }
    public int? PuntosPartida { get; set; }
    public int? GanadorPartida { get; set; }
    public bool? EsElo { get; set; }
}
