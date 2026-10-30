using AS.Domain.DTOs.Lista;

namespace AS.Domain.DTOs.Torneo;

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
    public int? IdEquipo1 { get; set; }
    public int? IdEquipo2 { get; set; }
    public string? NombreEquipo1 { get; set; }
    public string? NombreEquipo2 { get; set; }
    public int? IdCapitan1 { get; set; }
    public int? IdCapitan2 { get; set; }
    public ListaDTO? ListaJugador1 { get; set; }
    public ListaDTO? ListaJugador2 { get; set; }
    public int? NumeroMesa { get; set; }
    // Solo en torneos a Parejas: compañeros de los capitanes IdUsuario1 e IdUsuario2
    public int? IdCompanero1 { get; set; }
    public int? IdCompanero2 { get; set; }
    public string? NickCompanero1 { get; set; }
    public string? NickCompanero2 { get; set; }
    // Vacíos si la pareja juega con una lista conjunta
    public string? EjercitoCompanero1 { get; set; }
    public string? EjercitoCompanero2 { get; set; }
    // Solo en Copa: fase, grupo, cruce del cuadro y quién pasa si acaba en empate
    public string? Fase { get; set; }
    public int? Grupo { get; set; }
    public int? Cruce { get; set; }
    public int? IdUsuarioPasa { get; set; }
    // Solo en ligas: ELO ganado o perdido en la partida; null si no ha contado
    public int? EloVariacionUsuario1 { get; set; }
    public int? EloVariacionUsuario2 { get; set; }
}
