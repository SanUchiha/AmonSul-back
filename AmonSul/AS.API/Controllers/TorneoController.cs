using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.PartidaTorneo;
using AS.Application.DTOs.Torneo;
using AS.Application.Interfaces;
using AS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AS.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class TorneoController(
    ITorneoApplication torneoApplication, 
    IPartidaTorneoApplication partidaTorneoApplication) : ControllerBase
{
    private readonly ITorneoApplication _torneoApplication = torneoApplication;
    private readonly IPartidaTorneoApplication _partidaTorneoApplication = partidaTorneoApplication;

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetTorneos()
    {
        var response = await _torneoApplication.GetTorneos();

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("id/{idtorneo}")]
    public async Task<IActionResult> GetTorneo(int idtorneo)
    {
        var response = await _torneoApplication.GetById(idtorneo);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpPost]
    [Route("")]
    //[AllowAnonymous]
    [ProducesResponseType(typeof(bool), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearTorneo([FromBody, Required] CrearTorneoDTO request)
    {
        var response = await _torneoApplication.Register(request);

        if (response == false) return BadRequest("No se ha podido crear la partida");

        return Created(string.Empty, "La partida ha sido creada con éxito");
    }

    [HttpGet]
    [Route("bases/{idTorneo}")]
    public async Task<IActionResult> GetBasesTorneo(int idTorneo)
    {
        try
        {
            var (fileBytes, fileName) = await _torneoApplication.GetBasesTorneo(idTorneo);

            return File(fileBytes, "application/pdf", fileName);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error interno del servidor: " + ex.Message);
        }
    }

    [HttpPost]
    [Route("Gestion/Generar-Ronda")]
    //[AllowAnonymous]
    [ProducesResponseType(typeof(bool), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerarRonda([FromBody, Required] GenerarRondaDTO request)
    {
        bool response = await _partidaTorneoApplication.GenerateRound(request);

        if (response == false) return BadRequest("No se ha podido generar la ronda");

        return Created(string.Empty, "La ronda ha sido creada con éxito");
    }

    [HttpGet]
    //[AllowAnonymous]
    [Route("Gestion/Creados/{idUsuario}")]
    public async Task<IActionResult> GetTorneosCreadosUsuario(int idUsuario)
    {
        List<TorneoCreadoUsuarioDTO> response = await _torneoApplication.GetTorneosCreadosUsuario(idUsuario);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    //[AllowAnonymous]
    [Route("Gestion/info-torneo/{idTorneo}")]
    public async Task<IActionResult> GetInfoTorneoCreado(int idTorneo)
    {
        TorneoGestionInfoDTO response = await _torneoApplication.GetInfoTorneoCreado(idTorneo);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("Gestion/Partidas/{idTorneo}")]
    public async Task<IActionResult> GetPartidasTorneo(int idTorneo)
    {
        List<PartidaTorneoDTO> response = await _partidaTorneoApplication.GetPartidasTorneo(idTorneo);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpPut]
    [Route("Editar-Partida")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidarPartidaAmistosa([FromBody, Required] UpdatePartidaTorneoDTO request)
    {
        var response = await _partidaTorneoApplication.Edit(request);

        if (response == false) return BadRequest("No se ha podido editar la partida");

        return Ok(response);
    }
}
