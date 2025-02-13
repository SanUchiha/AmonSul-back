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
}
