namespace AS.Application.DTOs.Torneo;

public class TorneoCreadoDTO
{
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public string NombreTorneo { get; set; } = null!;
    public int? LimiteParticipantes { get; set; }
    public int NumeroPartidas { get; set; }
    public string? TipoTorneo { get; set; }
    public int ListasPorJugador { get; set; }
    public DateOnly? FechaEntregaListas { get; set; }
    public DateOnly? FechaFinInscripcion { get; set; }
    public DateOnly? InicioInscripciones { get; set; }
    public int? JugadoresXEquipo { get; set; }
    public bool MostrarListas { get; set; }
    public bool MostrarClasificacion { get; set; }
    public bool ListaPorPareja { get; set; }
    public string FormatoTorneo { get; set; } = null!;
    public bool CuentaElo { get; set; }
    public bool IdaYVuelta { get; set; }
    // Solo en Copa
    public int? NumeroGrupos { get; set; }
    public int? ClasificadosPorGrupo { get; set; }
    public string? RepartoGrupos { get; set; }
    public string? DesempateEliminatoria { get; set; }
    public bool TercerPuesto { get; set; }
}
