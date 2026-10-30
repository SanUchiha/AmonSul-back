namespace AS.Domain.DTOs.Torneo;

public class TorneoViewDTO
{
    public int IdTorneo { get; set; }
    public string NombreTorneo { get; set; } = null!;
    public int? LimiteParticipantes { get; set; }
    // Para Mis eventos: del primer al último día y si está en curso
    public DateOnly FechaInicioTorneo { get; set; }
    public DateOnly FechaFinTorneo { get; set; }
    public int NumeroPartidas { get; set; }
    public int PuntosTorneo { get; set; }
    public string? TipoTorneo { get; set; }
    public int ListasPorJugador { get; set; }
    public string FormatoTorneo { get; set; } = null!;
}
