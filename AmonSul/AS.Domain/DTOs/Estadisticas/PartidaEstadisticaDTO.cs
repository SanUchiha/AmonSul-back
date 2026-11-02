namespace AS.Domain.DTOs.Estadisticas;

/// <summary>
/// Una partida validada con los dos ejércitos, de torneo o amistosa, para las estadísticas de ejércitos. Los nombres de
/// ejército van tal cual están guardados: se unifican después
/// </summary>
public class PartidaEstadisticaDTO
{
    public int IdPartida { get; set; }
    public bool EsAmistosa { get; set; }
    public int? IdTorneo { get; set; }
    public string? NombreTorneo { get; set; }
    public int? Ronda { get; set; }
    public DateOnly Fecha { get; set; }
    // Los de la partida o, si no los tiene, los del torneo
    public int? Puntos { get; set; }
    public int IdUsuario1 { get; set; }
    public int IdUsuario2 { get; set; }
    public string Ejercito1 { get; set; } = "";
    public string Ejercito2 { get; set; } = "";
    public int Resultado1 { get; set; }
    public int Resultado2 { get; set; }
    // Null si quedaron en empate
    public int? Ganador { get; set; }
    // Si cada uno mató al general del rival: solo en los torneos
    public bool General1 { get; set; }
    public bool General2 { get; set; }
}
