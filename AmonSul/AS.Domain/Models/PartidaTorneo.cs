namespace AS.Domain.Models;

public partial class PartidaTorneo
{
    public int IdPartidaTorneo { get; set; }
    public int IdTorneo { get; set; }
    public int IdUsuario1 { get; set; }
    public int IdUsuario2 { get; set; }
    public int? ResultadoUsuario1 { get; set; }
    public int? ResultadoUsuario2 { get; set; }
    public DateOnly? FechaPartida { get; set; }
    public bool? EsMatchedPlayPartida { get; set; }
    public string? EscenarioPartida { get; set; }
    public int? PuntosPartida { get; set; }
    public int? GanadorPartidaTorneo { get; set; }
    public virtual Torneo? IdTorneoNavigation { get; set; }
    public virtual Usuario? IdUsuario1Navigation { get; set; }
    public virtual Usuario? IdUsuario2Navigation { get; set; }
    public bool? EsElo { get; set; }
    public bool? PartidaValidadaUsuario1 { get; set; }
    public bool? PartidaValidadaUsuario2 { get; set; }
    public virtual Usuario? GanadorPartidaTorneoNavigation { get; set; }
    public string? EjercitoUsuario1 { get; set; }
    public string? EjercitoUsuario2 { get; set; }
    public int? NumeroRonda  { get; set; }
    public bool? LiderMuertoUsuario1 { get; set; }
    public bool? LiderMuertoUsuario2 { get; set; }
    public int? IdEquipo1 { get; set; }
    public int? IdEquipo2 { get; set; }
    public virtual Equipo? IdEquipo1Navigation { get; set; }
    public virtual Equipo? IdEquipo2Navigation { get; set; }
}
