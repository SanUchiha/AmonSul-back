namespace AS.Domain.Models;

public partial class RangoTorneo
{
    public int IdRango { get; set; }

    public string NombreRango { get; set; } = null!;

    public int Puntos1 { get; set; }

    public int Puntos2 { get; set; }

    public int Puntos3 { get; set; }

    public int Puntos4 { get; set; }

    public int Puntos5 { get; set; }

    public int Puntos6 { get; set; }

    public int Puntos7 { get; set; }

    public int Puntos8 { get; set; }

    public int Puntos9 { get; set; }

    public int Puntos10 { get; set; }

    public virtual ICollection<Torneo> Torneos { get; set; } = new List<Torneo>();
}
