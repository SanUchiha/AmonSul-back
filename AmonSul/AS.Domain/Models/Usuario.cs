namespace AS.Domain.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = null!;
    public string PrimerApellido { get; set; } = null!;
    public string? SegundoApellido { get; set; }
    public string Email { get; set; } = null!;
    public string Contraseña { get; set; } = null!;
    public string Nick { get; set; } = null!;
    public string? NickLGDA { get; set; }
    public string? Ciudad { get; set; }
    public DateOnly FechaRegistro { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public int? IdFaccion { get; set; }
    public string? Telefono { get; set; }
    public bool? ProteccionDatos { get; set; }
    public string? Imagen { get; set; }

    public virtual ICollection<ClasificacionGeneral> ClasificacionGenerals { get; set; } = [];
    public virtual ICollection<ClasificacionTorneo> ClasificacionTorneos { get; set; } = [];
    public virtual ICollection<Comentario> Comentarios { get; set; } = [];
    public virtual ICollection<Elo> Elos { get; set; } = [];
    public virtual Faccion? IdFaccionNavigation { get; set; }
    public virtual ICollection<InscripcionTorneo> InscripcionTorneos { get; set; } = [];
    public virtual ICollection<PartidaAmistosa> PartidaAmistosaGanadorPartidaNavigations { get; set; } = [];
    public virtual ICollection<PartidaAmistosa> PartidaAmistosaIdUsuario1Navigations { get; set; } = [];
    public virtual ICollection<PartidaAmistosa> PartidaAmistosaIdUsuario2Navigations { get; set; } = [];
    public virtual ICollection<PartidaTorneo> PartidaTorneoIdUsuario1Navigations { get; set; } = [];
    public virtual ICollection<PartidaTorneo> PartidaTorneoIdUsuario2Navigations { get; set; } = [];
    public virtual ICollection<PartidaTorneo> PartidaTorneoGanadorPartidaNavigations { get; set; } = [];
    public virtual ICollection<Torneo> Torneos { get; set; } = [];
}
