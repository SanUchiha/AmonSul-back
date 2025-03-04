namespace AS.Application.DTOs.Lista;

public class CreateListaTorneoDTO
{
    public int IdInscripcion { get; set; }
    public int IdUsuario { get; set; }
    public int IdTorneo { get; set; }
    public required string ListaData { get; set; }
    public required string EmailOrganizador { get; set; }
    public required ArmyDTO Ejercito { get; set; }
    public DateOnly? FechaEntrega { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string? Nick { get; set; }
    public string? NombreEquipo { get; set; }
}
