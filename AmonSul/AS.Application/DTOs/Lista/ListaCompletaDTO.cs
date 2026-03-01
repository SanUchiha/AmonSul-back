namespace AS.Application.DTOs.Lista;

public class ListaCompletaDTO
{
    public int IdLista { get; set; }
    public int IdInscripcion { get; set; }
    public string Nick { get; set; } = string.Empty;
    public string? ListaData { get; set; }
    public string? Bando { get; set; }
    public string? Ejercito { get; set; }
    public string? EstadoLista { get; set; }
    public DateOnly? FechaEntrega { get; set; }
}
