namespace AS.Domain.DTOs.Circuitos;

/// <summary>
/// Un circuito en la lista: con los años de sus eventos, para filtrar por año, y cuántos tiene
/// </summary>
public class CircuitoListadoDTO
{
    public int IdCircuito { get; set; }
    public required string NombreCircuito { get; set; }
    // Quien lo creó desde la web: solo él lo renombra o lo borra
    public int? IdUsuario { get; set; }
    public List<int> Anios { get; set; } = [];
    public int NumeroEventos { get; set; }
}
