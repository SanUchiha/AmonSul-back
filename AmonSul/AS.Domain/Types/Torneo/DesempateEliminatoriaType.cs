namespace AS.Domain.Types.Torneo;

/// <summary>
/// Quién pasa en la eliminatoria de la Copa si la partida acaba en empate; lo elige el organizador
/// </summary>
public static class DesempateEliminatoriaType
{
    // El mejor clasificado en la fase de grupos
    public const string GRUPOS = "Grupos";
    // Quien mató al general enemigo; si lo mataron los dos o ninguno, el mejor de grupos
    public const string GENERAL = "General";
    // Lo decide el organizador en cada empate
    public const string ORGANIZADOR = "Organizador";

    public static bool EsValido(string? desempate) => desempate is GRUPOS or GENERAL or ORGANIZADOR;
}
