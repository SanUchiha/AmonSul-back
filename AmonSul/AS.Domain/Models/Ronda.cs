namespace AS.Domain.Models;

public partial class Ronda
{
    public int IdRonda { get; set; }
    public int? IdTorneo { get; set; }
    public int? NumeroRonda { get; set; }
    public string? EstadoRonda { get; set; }
    public DateOnly? FechaInicioRonda { get; set; }
    public DateOnly? FechaFinRonda { get; set; }
    public TimeOnly? HoraInicioRonda { get; set; }
    public int? DuracionRonda { get; set; }
    public string? EscenarioRonda { get; set; }
    public virtual Torneo? IdTorneoNavigation { get; set; }
}
