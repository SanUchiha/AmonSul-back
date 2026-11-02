namespace AS.Domain.DTOs.Estadisticas;

/// <summary>
/// El puesto final de un jugador en un torneo individual: para contar los torneos que gana y los podios de cada ejército
/// </summary>
public class PosicionTorneoDTO
{
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public int Puesto { get; set; }
    public DateOnly Fecha { get; set; }
    public int Puntos { get; set; }
}
