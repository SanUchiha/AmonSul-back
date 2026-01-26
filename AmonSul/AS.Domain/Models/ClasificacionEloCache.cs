namespace AS.Domain.Models;

public class ClasificacionEloCache
{
    public int IdClasificacion { get; set; }
    public int IdUsuario { get; set; }
    public required string Nick { get; set; }
    public int IdFaccion { get; set; }
    public int Elo { get; set; }
}