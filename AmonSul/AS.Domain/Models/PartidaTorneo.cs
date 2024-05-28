using System;
using System.Collections.Generic;

namespace AS.Domain.Models;

public partial class PartidaTorneo
{
    public int IdPartidaTorneo { get; set; }

    public int? IdTorneo { get; set; }

    public int? IdUsuario1 { get; set; }

    public int? IdUsuario2 { get; set; }

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
}
