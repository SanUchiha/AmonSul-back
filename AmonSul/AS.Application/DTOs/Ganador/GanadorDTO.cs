﻿namespace AS.Application.DTOs.Ganador;

public class GanadorDTO
{
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public int Resultado { get; set; }
    public string? Nick { get; set; }
    public string? NombreTorneo { get; set; }
}
