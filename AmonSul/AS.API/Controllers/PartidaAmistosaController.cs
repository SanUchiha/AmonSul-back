using AS.API.Extensions;
using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.Interfaces;
using AS.Utils.Constantes;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AS.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class PartidaAmistosaController(IPartidaAmistosaApplication partidaAmistosaApplication) : ControllerBase
{
    private readonly IPartidaAmistosaApplication _partidaAmistosaApplication = partidaAmistosaApplication;

    // Cada uno registra, valida y cancela sus propias partidas. 400 y no 403: el interceptor del front manda a la
    // página de error con cualquier 403
    private bool EsAdministracion(int idSolicitante) => ConstAdministracion.EsAdministrador(idSolicitante);

    /// <summary>
    /// Obtiene las partidas amistosas de un jugador
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("partidas/{idUser}")]
    [ProducesResponseType(typeof(List<ViewPartidaAmistosaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartidasAmistosasById(int idUser)
    {
        List<ViewPartidaAmistosaDTO> response = 
            await _partidaAmistosaApplication.GetPartidasAmistosasByUser(idUser);

        return Ok(response);
    }

    [HttpGet]
    [Route("Todas")]
    [ProducesResponseType(typeof(List<ViewPartidaAmistosaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartidasAmistosas()
    {
        var response = await _partidaAmistosaApplication.GetPartidasAmistosas();
        
        if(response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("Validadas")]
    [ProducesResponseType(typeof(List<ViewPartidaAmistosaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartidasAmistosasValidadas()
    {
        var response = await _partidaAmistosaApplication.GetPartidasAmistosasValidadas();

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("partida/{idPartida}")]
    [ProducesResponseType(typeof(ViewPartidaAmistosaDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(int idPartida)
    {
        var response = await _partidaAmistosaApplication.GetById(idPartida);

        if (response == null) return NotFound();

        return Ok(response);
    }

    /// <summary>
    /// Registrar una partida amistosa
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearPartidaAmistosa([FromBody, Required] CreatePartidaAmistosaDTO request)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        if (idSolicitante != request.IdUsuario1 && idSolicitante != request.IdUsuario2 && !EsAdministracion(idSolicitante))
            return BadRequest("Solo puedes registrar partidas en las que juegues.");
        // Cada jugador valida su lado: quien no es el jugador 1 no la deja validada por él
        if (idSolicitante != request.IdUsuario1)
            request.PartidaValidadaUsuario1 = null;

        bool response = await _partidaAmistosaApplication.Register(request);

        if (response == false) 
            return BadRequest("No se ha podido crear la partida");

        return CreatedAtAction(
            nameof(CrearPartidaAmistosa), 
            new { result = response}, 
            "La partida ha sido creada con exito");
    }

    [HttpPut]
    [Route("Validar")]
    public async Task<IActionResult> ValidarPartidaAmistosa([FromBody, Required] ValidarPartidaDTO request)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        string? miCorreo = User.FindFirst(ClaimTypes.Email)?.Value;
        if (!EsAdministracion(idSolicitante)
            && !string.Equals(miCorreo?.Trim(), request.EmailJugador?.Trim(), StringComparison.OrdinalIgnoreCase))
            return BadRequest("Solo puedes validar tus propias partidas.");

        bool response = await _partidaAmistosaApplication.ValidarPartidaAmistosa(request);

        if (response == false) return BadRequest("No se ha podido validar la partida");

        return Ok(response);
    }

    [HttpDelete]
    [Route("Cancelar/{idPartida}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelarPartida(int idPartida)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        ViewPartidaAmistosaDTO? partida = await _partidaAmistosaApplication.GetById(idPartida);
        if (partida is null)
            return BadRequest("La partida no existe.");
        if (idSolicitante != partida.IdUsuario1 && idSolicitante != partida.IdUsuario2 && !EsAdministracion(idSolicitante))
            return BadRequest("Solo los jugadores de la partida pueden cancelarla.");

        var response = await _partidaAmistosaApplication.Delete(idPartida);

        if (response == false) return BadRequest("No se ha podido cancelar la partida");

        return Ok(response);
    }

    [HttpGet]
    [Route("{email}")]
    [ProducesResponseType(typeof(List<ViewPartidaAmistosaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartidaAmistosasByUsuario(string email)
    {
        List<ViewPartidaAmistosaDTO> response = await _partidaAmistosaApplication.GetPartidaAmistosasByUsuario(email);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("Validadas/{idUsuario}")]
    [ProducesResponseType(typeof(List<ViewPartidaAmistosaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartidaAmistosasByUsuarioValidadas(int idUsuario)
    {
        List<ViewPartidaAmistosaDTO> response = await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(idUsuario);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("Pendientes/{idUsuario}")]
    [ProducesResponseType(typeof(List<ViewPartidaAmistosaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartidaAmistosasByUsuarioPendientes(int idUsuario)
    {
        List<ViewPartidaAmistosaDTO> response = await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioPendientes(idUsuario);

        if (response == null) return NotFound();

        return Ok(response);
    }

}
