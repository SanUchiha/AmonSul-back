namespace AS.Domain.Models;

public class Circuito
{
    public int IdCircuito { get; set; }
    public required string NombreCircuito { get; set; }

    public virtual ICollection<CircuitoTorneo>? CircuitoTorneos { get; set; } = [];
}