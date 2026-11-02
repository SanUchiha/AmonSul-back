namespace AS.Application.DTOs.Estadisticas;

/// <summary>
/// Todo de un ejército: su balance, según los puntos y la temporada, contra qué rivales y quién lo lleva
/// </summary>
public class FichaEjercitoDTO
{
    public EjercitoBalanceDTO Resumen { get; set; } = new();
    // En qué porcentaje de sus partidas de torneo mató al general rival; null si solo tiene amistosas
    public double? GeneralesMatados { get; set; }
    public List<TramoBalanceDTO> PorTramo { get; set; } = [];
    public List<TemporadaBalanceDTO> PorTemporada { get; set; } = [];
    // De mejor a peor porcentaje, solo los cruces con suficientes partidas
    public List<EjercitoBalanceDTO> Rivales { get; set; } = [];
    // De mejor a peor «Rinde», solo quien lo ha llevado bastantes veces
    public List<JugadorBalanceDTO> Jugadores { get; set; } = [];
}
