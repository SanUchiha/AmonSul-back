namespace AS.Domain.Models;

public partial class InscripcionTorneo
{
    public int IdInscripcion { get; set; }
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public string? EstadoInscripcion { get; set; }
    public DateOnly? FechaInscripcion { get; set; }
    public string? EstadoLista { get; set; }
    public DateOnly? FechaEntregaLista { get; set; }
    public string? EsPago { get; set; }
    public int? IdEquipo { get; set; }
    public int PuntosExtra { get; set; }
    // Solo en Liga y Copa: el ejército que elige al inscribirse y lleva todo el torneo
    public string? EjercitoElegido { get; set; }
    // Solo en Copa: su grupo, del 1 al número de grupos; null hasta que se reparten
    public int? Grupo { get; set; }

    public virtual Torneo? IdTorneoNavigation { get; set; }
    public virtual Usuario? IdUsuarioNavigation { get; set; }
    public virtual IList<Lista> Lista { get; set; } = [];
    public virtual Equipo? IdEquipoNavigation { get; set; } = null!;
}
