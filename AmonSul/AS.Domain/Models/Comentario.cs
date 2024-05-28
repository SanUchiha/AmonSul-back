using System;
using System.Collections.Generic;

namespace AS.Domain.Models;

public partial class Comentario
{
    public int IdComentario { get; set; }

    public int? IdTorneo { get; set; }

    public int? IdUsuario { get; set; }

    public int? PuntuarTorneo { get; set; }

    public string? Comentario1 { get; set; }

    public DateOnly? FechaComentario { get; set; }

    public virtual Torneo? IdTorneoNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
