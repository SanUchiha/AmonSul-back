using System;
using System.Collections.Generic;

namespace AS.Domain.Models;

public partial class ClasificacionTorneo
{
    public int IdClasificacionTorneo { get; set; }

    public int? IdTorneo { get; set; }

    public int? IdUsuario { get; set; }

    public int? PosicionFinal { get; set; }

    public int? PuntosTorneo { get; set; }

    public int? PuntosFavor { get; set; }

    public int? PuntosContra { get; set; }

    public int? PuntosGeneral { get; set; }

    public virtual Torneo? IdTorneoNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
