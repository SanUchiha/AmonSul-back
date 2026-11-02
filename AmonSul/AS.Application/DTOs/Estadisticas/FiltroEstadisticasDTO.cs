namespace AS.Application.DTOs.Estadisticas;

/// <summary>
/// Filtros comunes de las estadísticas de ejércitos. Sin ninguno: todas las temporadas y todos los puntos, sin amistosas.
/// Temporadas y tramos admiten varios a la vez (temporadas=2025&amp;temporadas=2026): vale cualquiera de ellos
/// </summary>
public class FiltroEstadisticasDTO
{
    public List<int>? Temporadas { get; set; }
    // «hasta-550», «600-650», «700-750» u «800+»
    public List<string>? Tramos { get; set; }
    // «good» o «evil»: solo para el ranking
    public string? Bando { get; set; }
    public bool Amistosas { get; set; }
}
