using System.ComponentModel.DataAnnotations.Schema;

namespace AS.Domain.Models;

[Table("Lista")]
public partial class Lista
{
    public int IdLista { get; set; }
    public int? IdInscripcion { get; set; }
    public string? ListaData { get; set; }
    public DateOnly? FechaEntrega { get; set; }
    public virtual InscripcionTorneo? IdInscripcionNavigation { get; set; }
}
