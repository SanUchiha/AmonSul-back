namespace AS.Domain.Models;

public class LigaTorneo
{
    public int IdLiga { get; set; }
    public Liga? Liga { get; set; }

    public int IdTorneo { get; set; }
    public Torneo? Torneo { get; set; }
}
