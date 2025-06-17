namespace AS.Application.DTOs.Torneo;

public class TorneoCreadoDTO
{
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public string NombreTorneo { get; set; } = null!;
    public int? LimiteParticipantes { get; set; }
    public int NumeroPartidas { get; set; }
    public int ListasPorJugador { get; set; }
    public DateOnly? FechaEntregaListas { get; set; }
    public DateOnly? FechaFinInscripcion { get; set; }
    public DateOnly? InicioInscripciones { get; set; }
    public int? JugadoresXEquipo { get; set; }
    public bool MostrarListas { get; set; }
}
