namespace AS.Application.DTOs.Estadisticas;

public class RatingEjercitoResponseDTO : EjercitoStatsDTO
{
    /// <summary>Indica si el resultado está filtrado por rivales concretos o bando rival.</summary>
    public bool EsFiltrado { get; set; }
    public string? DescripcionFiltro { get; set; }
}
