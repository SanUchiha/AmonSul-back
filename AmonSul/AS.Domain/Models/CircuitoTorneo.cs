namespace AS.Domain.Models;

public class CircuitoTorneo
{
    public int IdCircuito { get; set; }
    public Circuito? Circuito { get; set; }

    public int IdTorneo { get; set; }
    public Torneo? Torneo { get; set; }
}
