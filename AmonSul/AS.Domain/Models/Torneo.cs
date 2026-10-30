using AS.Domain.Types.Torneo;
using System.Reflection.Metadata.Ecma335;

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
    public ClassificationType ClassificationType { get; set; } = ClassificationType.NORMAL;
    public string? Contacto { get; set; }
    // Solo en torneos a Parejas: true si cada pareja entrega una lista conjunta en vez de una por jugador
    public bool ListaPorPareja { get; set; }
    public string FormatoTorneo { get; set; } = FormatoTorneoType.SUIZO;
    // Solo en Liga y Copa, los decide el organizador al crearla: si sus partidas cuentan para el ELO y si hay vuelta
    // (en la Copa, en los grupos)
    public bool CuentaElo { get; set; }
    public bool IdaYVuelta { get; set; }
    // Solo en Copa, los decide el organizador: cuántos grupos, cuántos pasan de cada uno, cómo se reparten, quién
    // pasa en un empate de la eliminatoria y si se juega el tercer puesto
    public int? NumeroGrupos { get; set; }
    public int? ClasificadosPorGrupo { get; set; }
    public string? RepartoGrupos { get; set; }
    public string? DesempateEliminatoria { get; set; }
    public bool TercerPuesto { get; set; }

    public virtual List<ClasificacionTorneo> ClasificacionTorneos { get; set; } = [];
    public virtual List<Comentario> Comentarios { get; set; } = [];
    public virtual RangoTorneo? IdRangoTorneoNavigation { get; set; }
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
    public virtual List<InscripcionTorneo> InscripcionTorneos { get; set; } = [];
    public virtual List<PartidaTorneo> PartidaTorneos { get; set; } = [];
    public virtual List<Ronda> Ronda { get; set; } = [];
    public virtual List<CircuitoTorneo>? CircuitoTorneos { get; set; } = [];
}
