namespace AS.Domain.DTOs.Estadisticas;

/// <summary>
/// Cuántas listas entregadas tienen ese ejército con ese bando: el bando de cada ejército sale de la mayoría
/// </summary>
public class EjercitoBandoDTO
{
    public string Ejercito { get; set; } = "";
    public string Bando { get; set; } = "";
    public int Listas { get; set; }
}
