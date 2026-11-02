using AS.API.Extensions;
using AS.Application.DTOs.Inscripcion;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Inscripcion;
using AS.Domain.Models;
using AS.Utils.Constantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class InscripcionController(
    IInscripcionApplication inscripcionApplication,
    ITorneoApplication torneoApplication
) : ControllerBase
{
    private readonly IInscripcionApplication _inscripcionApplication = inscripcionApplication;
    private readonly ITorneoApplication _torneoApplication = torneoApplication;

    // Una inscripción, con su lista aunque esté oculta: solo el inscrito, el organizador del torneo o la administración
    private async Task<bool> PuedeVerInscripcionAsync(int idTorneo, int idUsuarioInscrito) =>
        User.TryGetIdUsuario(out int idSolicitante)
        && (idSolicitante == idUsuarioInscrito
            || ConstAdministracion.EsAdministrador(idSolicitante)
            || await _torneoApplication.EsOrganizadorAsync(idTorneo, idSolicitante));

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InscripcionTorneo>>> GetInscripciones()
    {
        // Todas las inscripciones de todos: solo para la administración
        if (!User.TryGetIdUsuario(out int idSolicitante) || !ConstAdministracion.EsAdministrador(idSolicitante))
            return BadRequest("Solo la administración puede ver todas las inscripciones.");

        List<InscripcionTorneo> inscripciones = await _inscripcionApplication.GetInscripciones();
        return Ok(inscripciones);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InscripcionTorneoDTO>> GetInscripcionById(int id)
    {
        InscripcionTorneoDTO inscripcion = await _inscripcionApplication.GetInscripcionById(id);
        if (inscripcion == null)
        {
            return NotFound();
        }

        // 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
        if (!await PuedeVerInscripcionAsync(inscripcion.IdTorneo, inscripcion.IdUsuario))
            return BadRequest("Solo puedes ver tus propias inscripciones.");

        return Ok(inscripcion);
    }

    [HttpGet("Individual/byUser/{idUsuario}")]
    public async Task<ActionResult<List<InscripcionUsuarioIndividualDTO>>> GetInscripcionesIndividualByUser(int idUsuario)
    {
        List<InscripcionUsuarioIndividualDTO> inscripciones = 
            await _inscripcionApplication.GetInscripcionesIndividualByUser(idUsuario);
        
        return Ok(inscripciones);
    }

    [HttpGet("Equipo/byUser/{idUsuario}")]
    public async Task<ActionResult<List<InscripcionUsuarioEquipoDTO>>> GetInscripcionesEquipoByUser(int idUsuario)
    {
        List<InscripcionUsuarioEquipoDTO> inscripciones =
            await _inscripcionApplication.GetInscripcionEquipoByIdAsync(idUsuario);

        return Ok(inscripciones);
    }

    [HttpGet("ByTorneo/{idTorneo}")]
    public async Task<ActionResult<List<InscripcionUsuarioIndividualDTO>>> GetInscripcionesByTorneo(int idTorneo) =>
        Ok(await _inscripcionApplication.GetInscripcionesByTorneo(idTorneo));

    [HttpPost]
    public async Task<ActionResult> Register([FromBody] CrearInscripcionDTO inscripcionTorneo)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();

        // Uno se inscribe a sí mismo; a otros, solo el organizador (Añadir jugador)
        bool esOrganizador = await _inscripcionApplication.EsOrganizadorDelTorneoAsync(inscripcionTorneo.IdTorneo, idSolicitante);
        if (inscripcionTorneo.IdUsuario != idSolicitante && !esOrganizador)
            return BadRequest("Solo el organizador puede inscribir a otro jugador.");

        // Compartir el correo lo decide cada uno: el organizador no puede hacerlo por el jugador que añade
        if (inscripcionTorneo.IdUsuario != idSolicitante)
            inscripcionTorneo.CompartirEmail = null;

        // El pago y la lista los marca el organizador: un jugador no se inscribe ya pagado ni con la lista validada
        if (!esOrganizador)
        {
            inscripcionTorneo.EsPago = "NO";
            inscripcionTorneo.EstadoLista = "NO ENTREGADA";
            inscripcionTorneo.FechaInscripcion = DateOnly.FromDateTime(DateTime.Now);
        }

        bool result = await _inscripcionApplication.Register(inscripcionTorneo);
        if (!result)
        {
            return BadRequest();
        }
        return Created();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<InscripcionTorneo>> Delete(int id)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        // 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
        (bool puede, string? error) = await _inscripcionApplication.PuedeBorrarInscripcionAsync(id, idSolicitante);
        if (!puede)
            return BadRequest(error);

        InscripcionTorneo inscripcion = await _inscripcionApplication.Delete(id);
        if (inscripcion == null)
        {
            return NotFound();
        }
        return Ok(inscripcion);
    }

    [HttpDelete("Equipo/{idEquipo}")]
    public async Task<ActionResult<InscripcionTorneo>> DeleteEquipo(int idEquipo)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        if (!await _inscripcionApplication.EsCapitanUOrganizadorDeEquipoAsync(idEquipo, idSolicitante))
            return BadRequest("Solo el capitán o el organizador pueden borrar el equipo.");

        bool isDelete = await _inscripcionApplication.DeleteEquipo(idEquipo);
        return Ok(isDelete);
    }

    [HttpPut("Estado-Lista")]
    public async Task<ActionResult> CambiarEstadoLista([FromBody] ActualizarEstadoLista actualizarEstadoLista)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        if (!await _inscripcionApplication.EsOrganizadorDeInscripcionAsync(actualizarEstadoLista.IdInscripcion, idSolicitante))
            return BadRequest("Solo el organizador puede cambiar el estado de la lista.");

        var result = await _inscripcionApplication.CambiarEstadoLista(actualizarEstadoLista);
        if (!result)
        {
            return BadRequest();
        }
        return Created();
    }

    [HttpPut("Ejercito-Elegido")]
    public async Task<ActionResult> CambiarEjercitoElegido([FromBody] CambiarEjercitoElegidoDTO request)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();

        // 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
        (bool ok, string? error) = await _inscripcionApplication.CambiarEjercitoElegidoAsync(request, idSolicitante);
        return ok ? Ok() : BadRequest(error);
    }

    // Si el organizador puede ver su correo: lo cambia el propio jugador, en sus inscripciones
    [HttpPut("Compartir-Email")]
    public async Task<ActionResult> CambiarCompartirEmail([FromBody] CambiarCompartirEmailDTO request)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();

        // 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
        return await _inscripcionApplication.CambiarCompartirEmailAsync(request, idSolicitante)
            ? Ok()
            : BadRequest("Solo puedes cambiarlo en tus propias inscripciones.");
    }

    [HttpPut("Puntos-Extra")]
    public async Task<ActionResult> UpdatePuntosExtra([FromBody] UpdatePuntosExtraDTO request)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();

        // 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
        (bool ok, string? error) = await _inscripcionApplication.UpdatePuntosExtraAsync(request, idSolicitante);
        return ok ? Created() : BadRequest(error);
    }

    [HttpPut("Estado-Pago")]
    public async Task<ActionResult> CambiarEstadoPago([FromBody] ActualizarEstadoPago actualizarEstadoPago)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        if (!await _inscripcionApplication.EsOrganizadorDeInscripcionAsync(actualizarEstadoPago.IdInscripcion, idSolicitante))
            return BadRequest("Solo el organizador puede cambiar el estado del pago.");

        bool result = await _inscripcionApplication.CambiarEstadoPago(actualizarEstadoPago);
        if (!result)
        {
            return BadRequest();
        }
        return Created();
    }

    [HttpPut("Equipo/Estado-Pago")]
    public async Task<ActionResult> CambiarEstadoPagoEquipo([FromBody] ActualizarEstadoPagoEquipo actualizarEstadoPagoEquipo)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        if (!await _inscripcionApplication.EsOrganizadorDeEquipoAsync(actualizarEstadoPagoEquipo.IdEquipo, idSolicitante))
            return BadRequest("Solo el organizador puede cambiar el estado del pago.");

        bool result = await _inscripcionApplication.CambiarEstadoPagoEquipo(actualizarEstadoPagoEquipo);
        if (!result)
        {
            return BadRequest();
        }
        return Created();
    }

    [HttpPost("Equipo")]
    public async Task<IActionResult> CreateEquipo([FromBody] CreateEquipoDTO createEquipoDTO)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        // El capitán apunta su equipo; el organizador puede apuntar uno de otros
        if (createEquipoDTO.IdCapitan != idSolicitante
            && !await _inscripcionApplication.EsOrganizadorDelTorneoAsync(createEquipoDTO.IdTorneo, idSolicitante))
            return BadRequest("Solo puedes apuntar un equipo del que seas capitán.");

        // Compartir el correo lo decide cada uno: el organizador no puede hacerlo por el capitán
        if (createEquipoDTO.IdCapitan != idSolicitante)
            createEquipoDTO.CompartirEmail = null;

        return Ok(await _inscripcionApplication.CreaInscripcionEquipo(createEquipoDTO));
    }

    [HttpGet("Equipo/{idInscripcion}")]
    public async Task<ActionResult<List<InscripcionUsuarioEquipoDTO>>> GetInscripcionEquipo(int idInscripcion)
    {
        InscripcionEquipoDTO inscripcionEquipoDTO =
            await _inscripcionApplication.GetInscripcionEquipo(idInscripcion);

        return Ok(inscripcionEquipoDTO);
    }

    [HttpGet("Equipo/{idEquipo1}/{idEquipo2}")]
    public async Task<IActionResult> GetJugadpresEquipoParaCambioPairing(int idEquipo1, int idEquipo2)
    {
        List<JugadoresEquipoParaCambioDTO> equipos =
            await _inscripcionApplication.GetJugadoresEquipoParaCambioPairingAsync(idEquipo1, idEquipo2);

        return Ok(equipos);
    }

    [HttpGet("Apuntado/{idUsuario}/{idTorneo}")]
    public async Task<ActionResult<bool>> EstaApuntado(int idUsuario, int idTorneo)
    {
        bool esApuntado = await _inscripcionApplication.EstaApuntadoAsync(idUsuario, idTorneo);

        return Ok(esApuntado);
    }

    [HttpPost("Registar-miembro-Equipo")]
    public async Task<IActionResult> RegisterMiembro([FromBody] CreateMiembroEquipoDTO createMiembroEquipoDTO)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        if (!await _inscripcionApplication.EsCapitanUOrganizadorDeEquipoAsync(createMiembroEquipoDTO.IdEquipo, idSolicitante))
            return BadRequest("Solo el capitán o el organizador pueden añadir miembros al equipo.");

        return Ok(await _inscripcionApplication.RegisterMiembroAsync(createMiembroEquipoDTO));
    }

    [HttpDelete("Miembro/{idInscripcion}")]
    public async Task<ActionResult<InscripcionTorneo>> DeleteMiembro(int idInscripcion)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        if (!await _inscripcionApplication.PuedeTocarInscripcionAsync(idInscripcion, idSolicitante))
            return BadRequest("Solo el propio jugador, su capitán o el organizador pueden quitarlo del equipo.");

        return Ok(await _inscripcionApplication.DeleteMiembroAsync(idInscripcion));
    }
}
