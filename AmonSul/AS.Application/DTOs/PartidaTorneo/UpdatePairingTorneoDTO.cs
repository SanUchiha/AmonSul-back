namespace AS.Application.DTOs.PartidaTorneo;

public class UpdatePairingTorneoDTO
{
    public required int IdPartidaTorneo { get; set; }
    public int? IdUsuario1 { get; set; }
    public int? IdUsuario2 { get; set; }
    public string? EjercitoUsuario1 { get; set; }
    public string? EjercitoUsuario2 { get; set; }
}
