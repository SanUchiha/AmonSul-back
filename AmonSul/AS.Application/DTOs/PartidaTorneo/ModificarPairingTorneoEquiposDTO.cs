namespace AS.Application.DTOs.PartidaTorneo;

public class ModificarPairingTorneoEquiposDTO
{
    public int IdEquipo1Old { get; set; }
    public int IdEquipo2Old { get; set; }
    public int IdEquipo1 { get; set; }
    public int IdEquipo2 { get; set; }
    public int NumeroRonda { get; set; }
    public int IdTorneo{ get; set; }
}