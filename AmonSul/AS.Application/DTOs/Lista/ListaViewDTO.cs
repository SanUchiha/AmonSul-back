namespace AS.Application.DTOs.Lista;

public class ListaViewDTO
{
    public int IdLista { get; set; }
    public string? ListaData { get; set; }
    public DateOnly? FechaEntrega { get; set; }
    public string? Bando { get; set; }
    public string? Ejercito { get; set; }
}
