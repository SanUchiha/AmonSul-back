namespace AS.Application.DTOs.Estadisticas;

/// <summary>
/// Un ejército contra otro, o contra un bando, visto desde el primero
/// </summary>
public class CruceEjercitosDTO
{
    public string Ejercito { get; set; } = "";
    public string Rival { get; set; } = "";
    public BalanceDTO Resumen { get; set; } = new();
    public List<TramoBalanceDTO> PorTramo { get; set; } = [];
    // De la más reciente a la más antigua
    public List<PartidaCruceDTO> Partidas { get; set; } = [];
}

public class PartidaCruceDTO
{
    public int IdPartida { get; set; }
    public bool EsAmistosa { get; set; }
    public int? IdTorneo { get; set; }
    public string? NombreTorneo { get; set; }
    public int? Ronda { get; set; }
    public DateOnly Fecha { get; set; }
    public int? Puntos { get; set; }
    // El que lleva el ejército y su rival
    public int IdUsuario { get; set; }
    public string Nick { get; set; } = "";
    public string Ejercito { get; set; } = "";
    public int Resultado { get; set; }
    public int IdUsuarioRival { get; set; }
    public string NickRival { get; set; } = "";
    public string EjercitoRival { get; set; } = "";
    public int ResultadoRival { get; set; }
    // 1 si ganó el del ejército, 0,5 si empataron y 0 si perdió
    public double Puntuacion { get; set; }
}

public class CruceResumenDTO : BalanceDTO
{
    public string Ejercito { get; set; } = "";
    public string Rival { get; set; } = "";
}

/// <summary>
/// Los cruces más jugados y los más desequilibrados, cada uno visto desde el ejército que se impone
/// </summary>
public class CrucesDestacadosDTO
{
    public List<CruceResumenDTO> MasJugados { get; set; } = [];
    public List<CruceResumenDTO> MasDesequilibrados { get; set; } = [];
}
