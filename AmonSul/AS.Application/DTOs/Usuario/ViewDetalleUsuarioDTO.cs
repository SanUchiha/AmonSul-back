using AS.Application.DTOs.Elo;
using AS.Application.DTOs.PartidaAmistosa;

namespace AS.Application.DTOs.Usuario;

public class ViewDetalleUsuarioDTO
{
    public required string Nick { get; set; }
    public required string Email { get; set; }
    public string? IdFaccion { get; set; }
    public List<EloDTO> Elos { get; set; } = [];
    public int NumeroPartidasJugadas { get; set; }
    public int PartidasGanadas { get; set; }
    public int PartidasEmpatadas { get; set; }
    public int PartidasPerdidas { get; set; }
    public List<ViewPartidaAmistosaDTO> Partidas { get; set; } = [];
    public int ClasificacionElo { get; set; }
    public int PuntuacionElo { get; set; }
}
