using AS.Domain.Types.Torneo;

namespace AS.Application.DTOs.Torneo;

public class CrearTorneoDTO
{
    public int IdUsuario { get; set; } = 140;
    public string? NombreTorneo { get; set; }
    public string? DescripcionTorneo { get; set; }
    public int? LimiteParticipantes { get; set; }
    public DateOnly FechaInicioTorneo { get; set; }
    public DateOnly FechaFinTorneo { get; set; }
    public int PrecioTorneo { get; set; }
    public int NumeroPartidas { get; set; }
    public int PuntosTorneo { get; set; }
    public EstadoTorneoType EstadoTorneo { get; set; }
    public string LugarTorneo { get; set; } = null!;
    public string TipoTorneo { get; set; } = TorneoType.INDIVIDUAL;
    public int? IdRangoTorneo { get; set; }
    public int ListasPorJugador { get; set; }
    public bool? EsMatchedPlayTorneo { get; set; }
    public DateOnly? FechaEntregaListas { get; set; }
    public DateOnly? FechaFinInscripcion { get; set; }
    public string? BasesTorneo { get; set; }
    public string? CartelTorneo { get; set; }
    public string? MetodosPago { get; set; }
    public TimeOnly? HoraInicioTorneo { get; set; }
    public TimeOnly? HoraFinTorneo { get; set; }
    public DateOnly? InicioInscripciones { get; set; }
    public ClassificationType ClassificationType { get; set; } = ClassificationType.NORMAL;
    public string? Contacto { get; set; }
    // Solo en torneos a Parejas: true si cada pareja entrega una lista conjunta
    public bool ListaPorPareja { get; set; }
    public string FormatoTorneo { get; set; } = FormatoTorneoType.SUIZO;
    // Solo en Liga: si cuenta para el ELO y si es ida y vuelta
    public bool CuentaElo { get; set; }
    public bool IdaYVuelta { get; set; }
}
