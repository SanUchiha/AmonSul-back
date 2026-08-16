namespace AS.Application.DTOs.Torneo;

public class ResumenTorneoDTO
{
    public DateOnly Fecha { get; set; }
    public bool ListasVisibles { get; set; }
    public int NumeroRondas { get; set; }
    public List<bool> Rondas { get; set; } = [];
    public bool ClasificacionVisible { get; set; }
    public List<string> Ganadores { get; set; } = [];
}
