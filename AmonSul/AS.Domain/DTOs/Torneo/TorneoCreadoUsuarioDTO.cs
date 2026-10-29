namespace AS.Domain.DTOs.Torneo;

public class TorneoCreadoUsuarioDTO
{
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public string NombreTorneo { get; set; } = null!;
    public string TipoTorneo { get; set; } = null!;
    public int ListasPorJugador { get; set; }
    // Decide a qué vista de gestión se va
    public string FormatoTorneo { get; set; } = null!;
}
