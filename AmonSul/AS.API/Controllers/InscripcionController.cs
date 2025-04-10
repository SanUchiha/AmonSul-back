using AS.Application.DTOs.Inscripcion;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Inscripcion;
using AS.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class InscripcionController(IInscripcionApplication inscripcionApplication) : ControllerBase
{
    private readonly IInscripcionApplication _inscripcionApplication = inscripcionApplication;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InscripcionTorneo>>> GetInscripciones()
    {
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
        bool isDelete = await _inscripcionApplication.DeleteEquipo(idEquipo);
        return Ok(isDelete);
    }

    [HttpPut("Estado-Lista")]
    public async Task<ActionResult> CambiarEstadoLista([FromBody] ActualizarEstadoLista actualizarEstadoLista)
    {
        var result = await _inscripcionApplication.CambiarEstadoLista(actualizarEstadoLista);
        if (!result)
        {
            return BadRequest();
        }
        return Created();
    }

    [HttpPut("Estado-Pago")]
    public async Task<ActionResult> CambiarEstadoPago([FromBody] ActualizarEstadoPago actualizarEstadoPago)
    {
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
        bool result = await _inscripcionApplication.CambiarEstadoPagoEquipo(actualizarEstadoPagoEquipo);
        if (!result)
        {
            return BadRequest();
        }
        return Created();
    }

    [HttpPost("Equipo")]
    public async Task<IActionResult> CreateEquipo([FromBody] CreateEquipoDTO createEquipoDTO) => 
        Ok(await _inscripcionApplication.CreaInscripcionEquipo(createEquipoDTO));

    [HttpGet("Equipo/{idInscripcion}")]
    public async Task<ActionResult<List<InscripcionUsuarioEquipoDTO>>> GetInscripcionEquipo(int idInscripcion)
    {
        InscripcionEquipoDTO inscripcionEquipoDTO =
            await _inscripcionApplication.GetInscripcionEquipo(idInscripcion);

        return Ok(inscripcionEquipoDTO);
    }

    [HttpGet("Apuntado/{idUsuario}/{idTorneo}")]
    public async Task<ActionResult<bool>> EstaApuntado(int idUsuario, int idTorneo)
    {
        bool esApuntado = await _inscripcionApplication.EstaApuntadoAsync(idUsuario, idTorneo);

        return Ok(esApuntado);
    }

    [HttpPost("Registar-miembro-Equipo")]
    public async Task<IActionResult> RegisterMiembro([FromBody] CreateMiembroEquipoDTO createMiembroEquipoDTO) => 
        Ok(await _inscripcionApplication.RegisterMiembroAsync(createMiembroEquipoDTO));

    [HttpDelete("Miembro/{idInscripcion}")]
    public async Task<ActionResult<InscripcionTorneo>> DeleteMiembro(int idInscripcion) => 
        Ok(await _inscripcionApplication.DeleteMiembroAsync(idInscripcion));
}
