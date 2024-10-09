namespace AS.Application.DTOs.PartidaTorneo;

public class JugadorEmparejamientoDTO
{
    public int IdUsuario { get; set; }
    public required string Nick { get; set; }
    public int? IdFaccion { get; set; }
}