namespace AS.Application.DTOs.PartidaTorneo;

public class JugadorNarsilDTO
{
    public int IdUsuario { get; set; }
    public required string Nick { get; set; }
    public int? BandoAnterior { get; set; }
    public bool Emparejado { get; set; } = false;
}