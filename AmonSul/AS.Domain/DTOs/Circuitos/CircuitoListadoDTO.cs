namespace AS.Domain.DTOs.Circuitos;

/// <summary>
/// Un circuito en la lista: con su fecha de creación, para filtrar por año, y cuántos eventos tiene
/// </summary>
public class CircuitoListadoDTO
{
    public int IdCircuito { get; set; }
    public required string NombreCircuito { get; set; }
    // Quien lo creó desde la web: solo él lo renombra o lo borra
    public int? IdUsuario { get; set; }
    // Su año en la lista es el de su creación
    public DateOnly FechaCreacion { get; set; }
    public int NumeroEventos { get; set; }
}
