namespace AS.Application.DTOs.Inscripcion;

public class ComponentesEquipoDTO
{
    public int IdUsuario { get; set; }
    public int IdInscripcion { get; set; }
    public required string Nick { get; set; }
    public bool EsCapitan { get; set; }
    public string? EstadoLista { get; set; }
    public DateOnly? FechaEntregaLista { get; set; }
    public string? ListaData { get; set; }
    public string? Ejercito { get; set; }
    public int IdLista { get; set; }
}