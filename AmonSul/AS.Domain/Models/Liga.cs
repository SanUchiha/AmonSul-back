namespace AS.Domain.Models;

public class Liga
{
    public int IdLiga { get; set; }
    public required string NombreLiga { get; set; }

    public virtual ICollection<LigaTorneo>? LigaTorneos { get; set; } = [];
}