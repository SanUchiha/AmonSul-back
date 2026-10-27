using System.Text.Json.Serialization;

namespace AS.Application.DTOs.PartidaTorneo;

public class ViewPartidaTorneoDTO
{
    public int IdPartidaTorneo { get; set; }
    public int IdTorneo { get; set; }
    public int IdUsuario1 { get; set; }
    public string? NickUsuario1 { get; set; }
    public int IdUsuario2 { get; set; }
    public string? NickUsuario2 { get; set; }
    public int ResultadoUsuario1 { get; set; }
    public int ResultadoUsuario2 { get; set; }
    public DateOnly? FechaPartida { get; set; }
    public string? EscenarioPartida { get; set; }
    public int PuntosPartida { get; set; }
    public int? GanadorPartidaTorneo { get; set; }
    public string? GanadorPartidaNick { get; set; }
    public bool EsTorneo { get; set; }
    public string? EjercitoUsuario1 { get; set; }
    public string? EjercitoUsuario2 { get; set; }
    public string? NombreTorneo { get; set; }
    public int? NumeroRonda { get; set; }
    // Puntos del torneo: las partidas de torneo recientes no guardan PuntosPartida
    public int? PuntosTorneo { get; set; }
    // Si el organizador oculta las listas, el front no deja abrirlas desde la partida
    public bool? MostrarListas { get; set; }
    // ELO ganado o perdido en la partida; null si no contó para el ELO o no se sabe
    public int? EloVariacionUsuario1 { get; set; }
    public int? EloVariacionUsuario2 { get; set; }
    [JsonIgnore]
    public Domain.Models.Usuario? IdUsuario1Navigation { get; set; }
    [JsonIgnore]
    public Domain.Models.Usuario? IdUsuario2Navigation { get; set; }
}
