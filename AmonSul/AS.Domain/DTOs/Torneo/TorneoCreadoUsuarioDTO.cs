﻿namespace AS.Domain.DTOs.Torneo;

public class TorneoCreadoUsuarioDTO
{
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public string NombreTorneo { get; set; } = null!;
    public string TipoTorneo { get; set; } = null!;
    public int ListasPorJugador { get; set; }
}
