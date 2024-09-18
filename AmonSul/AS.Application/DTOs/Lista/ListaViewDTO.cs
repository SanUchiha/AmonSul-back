namespace AS.Application.DTOs.Lista;

public class ListaViewDTO
{
    public int IdLista { get; set; }
    public int? IdInscripcion { get; set; }
    public string? ListaData { get; set; }
    public DateOnly? FechaEntrega { get; set; }
}
