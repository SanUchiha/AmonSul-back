using System;
using System.Collections.Generic;

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

    public string? Ciudad { get; set; }

    public DateOnly FechaRegistro { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    public int? IdFaccion { get; set; }

    public string? Telefono { get; set; }

    public virtual ICollection<ClasificacionGeneral> ClasificacionGenerals { get; set; } = new List<ClasificacionGeneral>();

    public virtual ICollection<ClasificacionTorneo> ClasificacionTorneos { get; set; } = new List<ClasificacionTorneo>();

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual ICollection<Elo> Elos { get; set; } = new List<Elo>();

    public virtual Faccion? IdFaccionNavigation { get; set; }

    public virtual ICollection<InscripcionTorneo> InscripcionTorneos { get; set; } = new List<InscripcionTorneo>();

    public virtual ICollection<PartidaAmistosa> PartidaAmistosaGanadorPartidaNavigations { get; set; } = new List<PartidaAmistosa>();

    public virtual ICollection<PartidaAmistosa> PartidaAmistosaIdUsuario1Navigations { get; set; } = new List<PartidaAmistosa>();

    public virtual ICollection<PartidaAmistosa> PartidaAmistosaIdUsuario2Navigations { get; set; } = new List<PartidaAmistosa>();

    public virtual ICollection<PartidaTorneo> PartidaTorneoIdUsuario1Navigations { get; set; } = new List<PartidaTorneo>();

    public virtual ICollection<PartidaTorneo> PartidaTorneoIdUsuario2Navigations { get; set; } = new List<PartidaTorneo>();

    public virtual ICollection<Torneo> Torneos { get; set; } = new List<Torneo>();
}
