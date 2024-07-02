namespace AS.Domain.Models;

public partial class Torneo
{
    public int IdTorneo { get; set; }

    public int IdUsuario { get; set; }

    public string NombreTorneo { get; set; } = null!;

    public string DescripcionTorneo { get; set; } = null!;

    public int? LimiteParticipantes { get; set; }

    public DateOnly FechaInicioTorneo { get; set; }

    public DateOnly FechaFinTorneo { get; set; }

    public int PrecioTorneo { get; set; }

    public int NumeroPartidas { get; set; }

    public int PuntosTorneo { get; set; }

    public string EstadoTorneo { get; set; } = null!;

    public string LugarTorneo { get; set; } = null!;

    public string? TipoTorneo { get; set; }

    public bool EsPrivadoTorneo { get; set; }

    public bool EsLiga { get; set; }

    public int? IdRangoTorneo { get; set; }

    public bool? EsMatchedPlayTorneo { get; set; }

    public DateOnly? FechaEntregaListas { get; set; }

    public DateOnly? FechaFinInscripcion { get; set; }

    public byte[]? BasesTorneo { get; set; }

    public string? CartelTorneo { get; set; }

    public string? MetodosPago { get; set; }

    public TimeOnly? HoraInicioTorneo { get; set; }

    public TimeOnly? HoraFinTorneo { get; set; }

    public virtual ICollection<ClasificacionTorneo> ClasificacionTorneos { get; set; } = new List<ClasificacionTorneo>();

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual RangoTorneo? IdRangoTorneoNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<InscripcionTorneo> InscripcionTorneos { get; set; } = new List<InscripcionTorneo>();

    public virtual ICollection<PartidaTorneo> PartidaTorneos { get; set; } = new List<PartidaTorneo>();

    public virtual ICollection<Ronda> Ronda { get; set; } = new List<Ronda>();
}
