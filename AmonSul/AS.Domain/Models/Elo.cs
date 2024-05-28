using System;
using System.Collections.Generic;

namespace AS.Domain.Models;

public partial class Elo
{
    public int IdElo { get; set; }

    public int? IdUsuario { get; set; }

    public int? PuntuacionElo { get; set; }

    public virtual ICollection<ClasificacionGeneral> ClasificacionGenerals { get; set; } = new List<ClasificacionGeneral>();

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
