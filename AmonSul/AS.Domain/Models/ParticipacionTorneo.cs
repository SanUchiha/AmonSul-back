namespace AS.Domain.Models;

public class ParticipacionTorneo
{
    public int IdParticipacionTorneo { get; set; }

    public int IdTorneo { get; set; }
    public int IdUsuario { get; set; }
    public int IdInscripcion { get; set; }
    public int IdRonda { get; set; }
    public int IdBando { get; set; }

    public virtual Torneo? Torneo { get; set; }
    public virtual Usuario? Usuario { get; set; }
    public virtual InscripcionTorneo? Inscripcion { get; set; }
}
