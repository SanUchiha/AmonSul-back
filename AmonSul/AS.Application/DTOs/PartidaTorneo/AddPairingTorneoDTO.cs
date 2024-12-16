namespace AS.Application.DTOs.PartidaTorneo;

public class AddPairingTorneoDTO
{
    public int IdTorneo { get; set; }
    public int IdRonda { get; set; }
    public int IdUsuario1 { get; set; }
    public int IdUsuario2 { get; set; }
}
