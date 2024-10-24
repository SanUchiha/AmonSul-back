using AS.Application.DTOs.Elo;
using AS.Application.DTOs.Faccion;
using AS.Application.DTOs.Inscripcion;
using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.Torneo;

namespace AS.Application.DTOs.Usuario;

public class UsuarioDataDTO
{
    public int IdUsuario { get; set; }
    public required string Nick { get; set; }
    public required string Email { get; set; }
    public FaccionDTO? Faccion { get; set; }
    public List<ViewPartidaAmistosaDTO> PartidasValidadas { get; set; } = [];
    public List<ViewPartidaAmistosaDTO> PartidasPendientes { get; set; } = [];
    public List<EloDTO> Elos { get; set; } = [];
    public List<InscripcionUsuarioDTO> InscripcionesTorneo { get; set; } = [];
    public int ClasificacionElo { get; set; }
    public int PuntuacionElo { get; set; }
    public int NumeroPartidasJugadas { get; set; }
    public int PartidasGanadas { get; set; }
    public int PartidasEmpatadas { get; set; }
    public int PartidasPerdidas { get; set; }
    public bool? ProteccionDatos { get; set; }
    public List<ViewPartidaTorneoDTO> PartidasTorneo{ get; set; } = [];
    public List<ClasificacionTorneosDTO> ClasificacionTorneos{ get; set; } = [];
}
