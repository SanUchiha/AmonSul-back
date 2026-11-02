namespace AS.Application.DTOs.Estadisticas;

/// <summary>
/// Con qué ejércitos juega un jugador y cómo le va con cada uno, y contra qué ejércitos le cuesta más
/// </summary>
public class EjercitosJugadorDTO
{
    public int IdUsuario { get; set; }
    public string Nick { get; set; } = "";
    // Con todos sus ejércitos: para comparar cada uno con su media
    public BalanceDTO Resumen { get; set; } = new();
    // Del más jugado al menos
    public List<EjercitoBalanceDTO> Ejercitos { get; set; } = [];
    // Contra los que pierde más de lo que gana, del peor porcentaje al mejor, solo con suficientes partidas
    public List<EjercitoBalanceDTO> RivalesDificiles { get; set; } = [];
}
