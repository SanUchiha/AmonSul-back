namespace AS.Application.DTOs.Torneo;

public class InscripcionTorneoEmparejamientoDTO
{
    public int IdInscripcion { get; set; }
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public string? Ejercito { get; set; }
    public int IdLista { get; set; }
    public string? Nick { get; set; }
}