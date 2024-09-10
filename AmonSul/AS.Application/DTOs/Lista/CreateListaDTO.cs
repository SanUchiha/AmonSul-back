namespace AS.Application.DTOs.Lista;

public class CreateListaTorneoDTO
{
    public int IdInscripcion { get; set; }
    public required string ListaData { get; set; }
    public required string Ejercito { get; set; }
    public DateOnly? FechaEntrega { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}
