namespace AS.Domain.Types.Torneo;

/// <summary>
/// Fase de una partida de la Copa
/// </summary>
public static class FaseCopaType
{
    public const string GRUPOS = "Grupos";
    public const string ELIMINATORIA = "Eliminatoria";
    // Si el organizador lo pide, entre los que pierden las semifinales
    public const string TERCER_PUESTO = "TercerPuesto";
}
