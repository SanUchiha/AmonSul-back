using AS.Application.DTOs.Estadisticas;

namespace AS.Application.Interfaces;

public interface IEstadisticasApplication
{
    /// <summary>
    /// Devuelve el top 3 y bottom 3 de ejércitos por win rate, agrupado por bando (bien / oscuridad).
    /// </summary>
    Task<TopBandosResponseDTO> GetTopEjercitosPorBandoAsync(int top = 3);

    /// <summary>
    /// Devuelve el rating (stats) de un ejército concreto con filtros opcionales.
    /// </summary>
    Task<RatingEjercitoResponseDTO> GetRatingEjercitoAsync(RatingEjercitoRequestDTO request);

    // Las estadísticas de ejércitos nuevas. Null si el ejército no existe
    Task<List<EjercitoBalanceDTO>> GetRankingEjercitosAsync(FiltroEstadisticasDTO filtro, int minimo);
    Task<FichaEjercitoDTO?> GetFichaEjercitoAsync(string ejercito, FiltroEstadisticasDTO filtro);
    Task<CruceEjercitosDTO?> GetCruceAsync(string ejercito, string? rival, string? bandoRival, FiltroEstadisticasDTO filtro);
    Task<CrucesDestacadosDTO> GetCrucesDestacadosAsync(FiltroEstadisticasDTO filtro);
    Task<EjercitosJugadorDTO> GetEjercitosJugadorAsync(int idUsuario, FiltroEstadisticasDTO filtro);
}
