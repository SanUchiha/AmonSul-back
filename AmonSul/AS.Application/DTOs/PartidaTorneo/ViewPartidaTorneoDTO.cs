using System.Text.Json.Serialization;

namespace AS.Application.DTOs.PartidaTorneo;

public class ViewPartidaTorneoDTO
{
    public int IdPartidaTorneo { get; set; }
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
    [JsonIgnore]
    public Domain.Models.Usuario? IdUsuario1Navigation { get; set; }
    [JsonIgnore]
    public Domain.Models.Usuario? IdUsuario2Navigation { get; set; }
}
