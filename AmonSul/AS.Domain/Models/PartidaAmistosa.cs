using System;
using System.Collections.Generic;

namespace AS.Domain.Models;

public partial class PartidaAmistosa
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

    public virtual Usuario? GanadorPartidaNavigation { get; set; }

    public virtual Usuario? IdUsuario1Navigation { get; set; }

    public virtual Usuario? IdUsuario2Navigation { get; set; }
}
