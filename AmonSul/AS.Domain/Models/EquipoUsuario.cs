namespace AS.Domain.Models;

public class EquipoUsuario
{
    public int IdEquipo { get; set; }
    public int IdUsuario { get; set; }

    public Equipo Equipo { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
