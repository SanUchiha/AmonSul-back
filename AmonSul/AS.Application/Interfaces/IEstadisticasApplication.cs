using AS.Application.DTOs.Estadisticas;

namespace AS.Application.Interfaces;

public interface IEstadisticasApplication
{
    // Estadísticas de ejércitos. Null si el ejército no existe
    Task<List<EjercitoBalanceDTO>> GetRankingEjercitosAsync(FiltroEstadisticasDTO filtro, int minimo);
    Task<FichaEjercitoDTO?> GetFichaEjercitoAsync(string ejercito, FiltroEstadisticasDTO filtro);
    Task<CruceEjercitosDTO?> GetCruceAsync(string ejercito, string? rival, string? bandoRival, FiltroEstadisticasDTO filtro);
    Task<CrucesDestacadosDTO> GetCrucesDestacadosAsync(FiltroEstadisticasDTO filtro);
    Task<EjercitosJugadorDTO> GetEjercitosJugadorAsync(int idUsuario, FiltroEstadisticasDTO filtro);
}
