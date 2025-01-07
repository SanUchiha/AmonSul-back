namespace AS.Domain.Models;

public class LigaTorneo
{
    public int IdLiga { get; set; }
    public required Liga Liga { get; set; }

    public int IdTorneo { get; set; }
    public required Torneo Torneo { get; set; }
}
