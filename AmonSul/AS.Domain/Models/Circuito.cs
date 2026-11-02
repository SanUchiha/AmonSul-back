namespace AS.Domain.Models;

public class Circuito
{
    public int IdCircuito { get; set; }
    public required string NombreCircuito { get; set; }
    // Quien lo creó desde la web: solo él lo renombra o lo borra. Los antiguos y el anual de Amon Sûl no tienen
    public int? IdUsuario { get; set; }
    // Cuándo se creó: la web filtra los circuitos por su año. Los Amon Sûl, el 1 de enero de su año; los que ya
    // existían, el 1 de enero del año de su nombre
    public DateOnly FechaCreacion { get; set; }

    public virtual ICollection<CircuitoTorneo>? CircuitoTorneos { get; set; } = [];
}