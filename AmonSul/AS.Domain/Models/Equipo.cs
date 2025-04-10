namespace AS.Domain.Models;

public class Equipo
{
    public int IdEquipo { get; set; }
    public string NombreEquipo { get; set; } = null!;
    public int IdCapitan { get; set; }

    public virtual Usuario Capitan { get; set; } = null!;
    public virtual List<EquipoUsuario> Miembros { get; set; } = [];
    public virtual List<InscripcionTorneo> Inscripciones { get; set; } = [];
}
