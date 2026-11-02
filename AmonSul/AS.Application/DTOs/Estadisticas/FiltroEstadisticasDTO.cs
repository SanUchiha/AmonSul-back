namespace AS.Application.DTOs.Estadisticas;

/// <summary>
/// Filtros comunes de las estadísticas de ejércitos. Sin ninguno: todas las temporadas y todos los puntos, sin amistosas
/// </summary>
public class FiltroEstadisticasDTO
{
    public int? Temporada { get; set; }
    // «hasta-550», «600-650», «700-750» u «800+»
    public string? Tramo { get; set; }
    // «good» o «evil»: solo para el ranking
    public string? Bando { get; set; }
    public bool Amistosas { get; set; }
}
