namespace AS.Domain.Models;

public partial class Faccion
{
    public int IdFaccion { get; set; }
    public string NombreFaccion { get; set; } = null!;
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
