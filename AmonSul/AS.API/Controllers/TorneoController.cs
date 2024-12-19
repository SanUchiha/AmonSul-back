using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.PartidaTorneo;
using AS.Application.DTOs.Torneo;
using AS.Application.Interfaces;
using AS.Application.Services;
using AS.Domain.DTOs.Torneo;
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
    IPartidaTorneoApplication partidaTorneoApplication,
    IGanadorApplication ganadorApplication) : ControllerBase
{
    private readonly ITorneoApplication _torneoApplication = torneoApplication;
    private readonly IPartidaTorneoApplication _partidaTorneoApplication = partidaTorneoApplication;
    private readonly IGanadorApplication _ganadorApplication = ganadorApplication;

    #region Gestion torneo

    /// <summary>
    /// Generar Rondas
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("Gestion/Generar-Ronda")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerarRonda([FromBody, Required] GenerarRondaDTO request)
    {
        bool response = await _partidaTorneoApplication.GenerateRound(request);

        if (response == false) return BadRequest("No se ha podido generar la ronda");

        return Created(string.Empty, "La ronda ha sido creada con éxito");
    }

    [HttpGet]
    [Route("Gestion/Creados/{idUsuario}")]
    public async Task<IActionResult> GetTorneosCreadosUsuario(int idUsuario)
    {
        List<TorneoCreadoUsuarioDTO> response =
            await _torneoApplication.GetTorneosCreadosUsuario(idUsuario);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
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

    [HttpGet]
    [Route("Gestion/Partidas/{idTorneo}/{idRonda}")]
    public async Task<IActionResult> GetPartidasRondaTorneo(int idTorneo, int idRonda)
    {
        List<PartidaTorneoDTO> response = await _partidaTorneoApplication.GetPartidasTorneoByRonda(idTorneo, idRonda);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("Gestion/issave/{idTorneo}")]
    public async Task<IActionResult> IsSaveTournament(int idTorneo)
    {
        bool response = await _ganadorApplication.IsSave(idTorneo);
        
        return Ok(response);
    }

    #endregion

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

    /// <summary>
    /// Crear torneo
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> CrearTorneo([FromBody, Required] CrearTorneoDTO request)
    {
        bool response = await _torneoApplication.Register(request);

        if (response == false) return BadRequest("No se ha podido crear el torneo");

        return Created(string.Empty, "El torneo ha sido creada con éxito");
    }

    /// <summary>
    /// Obtener las bases de un torneo
    /// </summary>
    /// <param name="idTorneo"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Editar la partida de un torneo
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("Editar-Partida")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EdtarPartida([FromBody, Required] UpdatePartidaTorneoDTO request)
    {
        bool response = await _partidaTorneoApplication.Edit(request);

        if (response == false) return BadRequest("No se ha podido editar la partida");

        return Ok(response);
    }

    /// <summary>
    /// Editar el pairing de un torneo
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("Editar-Pairing")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EditarPairing([FromBody, Required] UpdatePairingTorneoDTO request)
    {
        bool response = await _partidaTorneoApplication.EdtarPairing(request);

        if (response == false) return BadRequest("No se ha podido editar la partida");

        return Ok(response);
    }

    /// <summary>
    /// Agregar un pairing a un torneo
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("Agregar-Pairing")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddPairing([FromBody, Required] AddPairingTorneoDTO request)
    {
        bool response = await _partidaTorneoApplication.Register(request);

        if (response == false) return BadRequest("No se ha podido agregar la partida");

        return Ok(response);
    }

    /// <summary>
    /// Eliminar una partida de un torneo
    /// </summary>
    /// <param name="idPartida"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("Eliminar-Partida/{idPartida}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePartida(int idPartida)
    {
        bool response = await _partidaTorneoApplication.Delete(idPartida);

        if (response == false) return BadRequest("No se ha podido eliminar la partida");

        return Ok(response);
    }

    /// <summary>
    /// Obtiene todas las partidas en torneos de un usuario
    /// </summary>
    /// <param name="idUsuario"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("Partidas/Usuario/{idUsuario}")]
    public async Task<IActionResult> GetPartidasUsuarioTorneos(int idUsuario)
    {
        List<ViewPartidaTorneoDTO> response =
            await _partidaTorneoApplication.GetPartidasTorneosByUsuario(idUsuario);

        if (response == null) return NotFound();

        return Ok(response);
    }
}
