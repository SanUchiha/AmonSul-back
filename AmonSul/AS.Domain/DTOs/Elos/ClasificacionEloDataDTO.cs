namespace AS.Domain.DTOs.Elos;

public class ClasificacionEloDataDTO
{
    public int IdUsuario { get; set; }
    public required string Nick { get; set; }
    public int Elo { get; set; }
    public int? IdFaccion { get; set; }
}
