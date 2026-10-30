namespace AS.Domain.Types.Torneo;

/// <summary>
/// Cómo reparte la Copa a los jugadores en los grupos; lo elige el organizador
/// </summary>
public static class RepartoGruposType
{
    // Los de más ELO, uno por grupo como cabezas de serie, y el resto por bombos al azar
    public const string ELO = "Elo";
    public const string SORTEO = "Sorteo";
    // El organizador coloca a cada jugador
    public const string MANUAL = "Manual";

    public static bool EsValido(string? reparto) => reparto is ELO or SORTEO or MANUAL;
}
