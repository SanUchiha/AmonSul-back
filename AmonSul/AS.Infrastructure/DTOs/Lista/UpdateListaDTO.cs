using AS.Application.DTOs.Lista;

namespace AS.Infrastructure.DTOs.Lista;

public class UpdateListaDTO
{
    public int IdUsuario { get; set; }
    public int IdOrganizador { get; set; }
    public int IdTorneo { get; set; }
    public int IdLista { get; set; }
    public required string ListaData { get; set; }
    public required ArmyDTO Ejercito { get; set; }
    public DateOnly? FechaEntrega { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}
