namespace AS.Domain.Types.Torneo;

/// <summary>
/// Cómo se empareja el torneo; TorneoType dice quién juega
/// </summary>
public static class FormatoTorneoType
{
    public const string SUIZO = "Suizo";
    // Todos contra todos por jornadas
    public const string LIGA = "Liga";
    // Fase de grupos (todos contra todos en cada grupo) y eliminatoria, como un mundial
    public const string COPA = "Copa";

    public static bool EsValido(string? formato) => formato is SUIZO or LIGA or COPA;

    // Liga y Copa: jornadas con calendario abierto, ejército fijo, lista guardada en cada partida y ELO partida a partida
    public static bool EsPorJornadas(string? formato) => formato is LIGA or COPA;
}
