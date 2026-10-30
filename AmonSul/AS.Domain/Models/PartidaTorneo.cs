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
    // Solo en torneos a Parejas: compañero del jugador 1 y del jugador 2, que son los capitanes
    public int? IdCompanero1 { get; set; }
    public int? IdCompanero2 { get; set; }
    // Vacíos si la pareja juega con una lista conjunta
    public string? EjercitoCompanero1 { get; set; }
    public string? EjercitoCompanero2 { get; set; }
    // Solo en Liga y Copa: la lista con la que se jugó, guardada al registrar el resultado. El jugador puede cambiar
    // de lista en la siguiente jornada y esta sigue siendo la de esa partida
    public string? ListaUsuario1 { get; set; }
    public string? ListaUsuario2 { get; set; }
    // Solo en Copa: fase (FaseCopaType), grupo en la fase de grupos, posición del cruce en su ronda de la
    // eliminatoria (de él sale con quién se cruza el ganador) y quién pasa si acaba en empate
    public string? Fase { get; set; }
    public int? Grupo { get; set; }
    public int? Cruce { get; set; }
    public int? IdUsuarioPasa { get; set; }
    public virtual Usuario? IdCompanero1Navigation { get; set; }
    public virtual Usuario? IdCompanero2Navigation { get; set; }
}
