namespace AS.Application.DTOs.Torneo;

public class ClasificacionTorneosDTO
{
    public int IdTorneo { get; set; }
    public string? NombreTorneo { get; set; }
    public int Resultado { get; set; }
    // Cuántos jugadores hay en la clasificación final: el 45 de «3/45»
    public int TotalJugadores { get; set; }
}
