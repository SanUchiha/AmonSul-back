namespace AS.Domain.Models;

public class ClasificacionEloCache
{
    public int IdClasificacion { get; set; }
    public int IdUsuario { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nick { get; set; } = string.Empty;
    public int? IdFaccion { get; set; }
    public int Elo { get; set; }
    public int Partidas { get; set; }
    public int Ganadas { get; set; }
    public int Empatadas { get; set; }
    public int Perdidas { get; set; }
    public int NumeroPartidasJugadas { get; set; }
    public int AnioClasificacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public bool Activo { get; set; } = true;

    // Navegación
    public virtual Usuario? Usuario { get; set; }
    public virtual Faccion? Faccion { get; set; }
}