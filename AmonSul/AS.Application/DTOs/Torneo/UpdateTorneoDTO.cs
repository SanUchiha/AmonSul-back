using AS.Domain.Types.Torneo;

namespace AS.Application.DTOs.Torneo;

public class UpdateTorneoDTO
{
    public int IdTorneo { get; set; }
    public int? LimiteParticipantes { get; set; }
    public DateOnly? FechaInicioTorneo { get; set; }
    public DateOnly? FechaFinTorneo { get; set; }
    public int? PrecioTorneo { get; set; }
    public int? NumeroPartidas { get; set; }
    public int? PuntosTorneo { get; set; }
    public string? LugarTorneo { get; set; } = null!;
    public DateOnly? FechaEntregaListas { get; set; }
    public DateOnly? FechaFinInscripcion { get; set; }
    public string? BasesTorneo { get; set; }
    public string? CartelTorneo { get; set; }
    public string? MetodosPago { get; set; }
    public TimeOnly? HoraInicioTorneo { get; set; }
    public TimeOnly? HoraFinTorneo { get; set; }
    public int ListasPorJugador { get; set; }
    public ClassificationType ClassificationType { get; set; }
    public required string TipoTorneo { get; set; }
    public string? Contacto { get; set; }
    // Solo en torneos a Parejas: true si cada pareja entrega una lista conjunta
    public bool? ListaPorPareja { get; set; }
    // Solo en Liga y Copa, y mientras no tenga partidas
    public bool? CuentaElo { get; set; }
    public bool? IdaYVuelta { get; set; }
    // Solo en Copa: grupos y reparto mientras no tenga partidas; el resto, hasta que empiece la eliminatoria
    public int? NumeroGrupos { get; set; }
    public int? ClasificadosPorGrupo { get; set; }
    public string? RepartoGrupos { get; set; }
    public string? DesempateEliminatoria { get; set; }
    public bool? TercerPuesto { get; set; }
}
