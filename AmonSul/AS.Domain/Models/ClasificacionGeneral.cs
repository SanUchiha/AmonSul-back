namespace AS.Domain.Models;

public partial class ClasificacionGeneral
{
    public int IdClasificacion { get; set; }

    public int? IdUsuario { get; set; }

    public int? PuntuacionTotal { get; set; }

    public int? IdPuntuacionElo { get; set; }

    public virtual Elo? IdPuntuacionEloNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
