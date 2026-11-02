namespace AS.Application.DTOs.Estadisticas;

/// <summary>
/// Cómo le ha ido a un ejército (o a un jugador, o en un cruce). El porcentaje cuenta el empate como media victoria y
/// «Rinde» es lo que ha ganado de más o de menos frente a lo que esperaba el ELO de los jugadores, en puntos de porcentaje
/// </summary>
public class BalanceDTO
{
    public int Partidas { get; set; }
    public int Victorias { get; set; }
    public int Empates { get; set; }
    public int Derrotas { get; set; }
    public double Porcentaje { get; set; }
    public double Rinde { get; set; }
    // Puntos de victoria por partida, a favor y en contra
    public double PuntosFavor { get; set; }
    public double PuntosContra { get; set; }
}

public class EjercitoBalanceDTO : BalanceDTO
{
    public string Ejercito { get; set; } = "";
    public string? Bando { get; set; }
    // Cuántos jugadores distintos lo han llevado
    public int Jugadores { get; set; }
}

public class TramoBalanceDTO : BalanceDTO
{
    public string Tramo { get; set; } = "";
}

public class TemporadaBalanceDTO : BalanceDTO
{
    public int Temporada { get; set; }
}

public class JugadorBalanceDTO : BalanceDTO
{
    public int IdUsuario { get; set; }
    public string Nick { get; set; } = "";
}
