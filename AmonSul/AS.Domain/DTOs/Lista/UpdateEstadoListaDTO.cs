namespace AS.Domain.DTOs.Lista;

public class UpdateEstadoListaDTO
{
    public int IdLista { get; set; }
    public required string Estado { get; set; }
    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
}
