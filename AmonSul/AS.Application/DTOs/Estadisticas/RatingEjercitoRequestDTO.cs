namespace AS.Application.DTOs.Estadisticas;

public class RatingEjercitoRequestDTO
{
    /// <summary>Nombre del ejército a consultar.</summary>
    public required string Ejercito { get; set; }

    /// <summary>Filtro de fecha inicio (inclusivo).</summary>
    public DateOnly? FechaDesde { get; set; }

    /// <summary>Filtro de fecha fin (inclusivo).</summary>
    public DateOnly? FechaHasta { get; set; }

    /// <summary>Ejércitos rivales contra los que filtrar. Si está vacío, se calculan todas las partidas.</summary>
    public List<string>? EjercitosRivales { get; set; }

    /// <summary>"good" o "evil". Filtra por bando del rival. Ignorado si EjercitosRivales tiene valores.</summary>
    public string? BandoRival { get; set; }

    /// <summary>Puntos mínimos de la partida (inclusivo). Ej: 500.</summary>
    public int? PuntosPartidaMin { get; set; }

    /// <summary>Puntos máximos de la partida (inclusivo). Ej: 700.</summary>
    public int? PuntosPartidaMax { get; set; }
}
