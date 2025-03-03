﻿using System.Text.Json.Serialization;

namespace AS.Application.DTOs.PartidaAmistosa;

public class ViewPartidaAmistosaDTO
{
    public int IdPartidaAmistosa { get; set; }
    public int IdUsuario1 { get; set; }
    public string? NickUsuario1 { get; set; }
    public int IdUsuario2 { get; set; }
    public string? NickUsuario2 { get; set; }
    public int ResultadoUsuario1 { get; set; }
    public int ResultadoUsuario2 { get; set; }
    public DateOnly? FechaPartida { get; set; }
    public bool? EsMatchedPlayPartida { get; set; }
    public string? EscenarioPartida { get; set; }
    public int PuntosPartida { get; set; }
    public int GanadorPartida { get; set; }
    public string? GanadorPartidaNick { get; set; }
    public bool EsElo { get; set; }
    public bool EsTorneo { get; set; }
    public string? EjercitoUsuario1 { get; set; }
    public string? EjercitoUsuario2 { get; set; }
    public bool? PartidaValidadaUsuario1 { get; set; }
    public bool? PartidaValidadaUsuario2 { get; set; }
    [JsonIgnore]
    public Domain.Models.Usuario? IdUsuario1Navigation { get; set; }
    [JsonIgnore]
    public Domain.Models.Usuario? IdUsuario2Navigation { get; set; }
}
