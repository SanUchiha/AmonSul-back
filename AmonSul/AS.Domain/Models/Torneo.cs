﻿using AS.Domain.Types;

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
    public string TipoTorneo { get; set; } = TorneoType.INDIVIDUAL;
    public bool EsPrivadoTorneo { get; set; }
    public bool EsLiga { get; set; }
    public int? IdRangoTorneo { get; set; }
    public bool? EsMatchedPlayTorneo { get; set; }
    public bool MostrarListas { get; set; }
    public bool MostrarClasificacion { get; set; }
    public DateOnly? FechaEntregaListas { get; set; }
    public DateOnly? FechaFinInscripcion { get; set; }
    public byte[]? BasesTorneo { get; set; }
    public string? CartelTorneo { get; set; }
    public string? MetodosPago { get; set; }
    public TimeOnly? HoraInicioTorneo { get; set; }
    public TimeOnly? HoraFinTorneo { get; set; }
    public DateOnly? InicioInscripciones { get; set; }
    public int ListasPorJugador { get; set; }

    public virtual List<ClasificacionTorneo> ClasificacionTorneos { get; set; } = [];
    public virtual List<Comentario> Comentarios { get; set; } = [];
    public virtual RangoTorneo? IdRangoTorneoNavigation { get; set; }
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
    public virtual List<InscripcionTorneo> InscripcionTorneos { get; set; } = [];
    public virtual List<PartidaTorneo> PartidaTorneos { get; set; } = [];
    public virtual List<Ronda> Ronda { get; set; } = [];
    public virtual List<LigaTorneo>? LigaTorneos { get; set; } = [];
}
