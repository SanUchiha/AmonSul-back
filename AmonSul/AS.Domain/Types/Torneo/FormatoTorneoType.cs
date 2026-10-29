namespace AS.Domain.Types.Torneo;

/// <summary>
/// Cómo se empareja el torneo; TorneoType dice quién juega
/// </summary>
public static class FormatoTorneoType
{
    public const string SUIZO = "Suizo";
    // Todos contra todos por jornadas
    public const string LIGA = "Liga";

    public static bool EsValido(string? formato) => formato is SUIZO or LIGA;
}
