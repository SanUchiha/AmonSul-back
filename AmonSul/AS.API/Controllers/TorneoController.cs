using AS.API.Filters;
using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.PartidaTorneo;
using AS.Application.DTOs.Torneo;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Torneo;
using Microsoft.AspNetCore.Authorization;
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

    [HttpPost]
    [Route("Gestion/Generar-Ronda")]
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
    [ServiceFilter(typeof(AdminTorneoFilter))]
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
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> GetPartidasRondaTorneo(int idTorneo, int idRonda)
    {
        List<PartidaTorneoDTO> response = await _partidaTorneoApplication.GetPartidasTorneoByRonda(idTorneo, idRonda);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("Gestion/issave/{idTorneo}")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> IsSaveTournament(int idTorneo)
    {
        bool response = await _ganadorApplication.IsSave(idTorneo);
        
        return Ok(response);
    }

    [HttpPatch]
    [Route("Gestion/editar")]
    public async Task<IActionResult> UpdateTorneo(
        [FromBody, Required] UpdateTorneoDTO request) => 
            Ok(await _torneoApplication.UpdateTorneoAsync(request));

    [HttpPatch]
    [Route("Gestion/subir-bases")]
    public async Task<IActionResult> UpdateBasesTorneo(
    [FromBody, Required] UpdateBasesDTO request) =>
        Ok(await _torneoApplication.UpdateBasesTorneoAsync(request));

    [HttpDelete]
    [Route("Gestion/{idTorneo}")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> DeleteTournament(int idTorneo)
    {
        bool response = await _torneoApplication.Delete(idTorneo);

        return Ok(response);
    }

    #endregion

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetTorneos()
    {
        List<TorneoDTO> response = await _torneoApplication.GetTorneos();

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("id/{idtorneo}")]
    public async Task<IActionResult> GetTorneo(int idtorneo)
    {
        TorneoDTO response = await _torneoApplication.GetById(idtorneo);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> CrearTorneo([FromBody, Required] CrearTorneoDTO request)
    {
        bool response = await _torneoApplication.Register(request);

        if (response == false) return BadRequest("No se ha podido crear el torneo");

        return Created(string.Empty, "El torneo ha sido creada con éxito");
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

    [HttpPut]
    [Route("Editar-Partida")]
    public async Task<IActionResult> EdtarPartida([FromBody, Required] UpdatePartidaTorneoDTO request)
    {
        bool response = await _partidaTorneoApplication.Edit(request);

        if (response == false) return BadRequest("No se ha podido editar la partida");

        return Ok(response);
    }

    [HttpPut]
    [Route("Editar-Pairing")]
    public async Task<IActionResult> EditarPairing([FromBody, Required] UpdatePairingTorneoDTO request)
    {
        bool response = await _partidaTorneoApplication.EdtarPairing(request);

        if (response == false) return BadRequest("No se ha podido editar la partida");

        return Ok(response);
    }

    [HttpPost]
    [Route("Agregar-Pairing")]
    public async Task<IActionResult> AddPairing([FromBody, Required] AddPairingTorneoDTO request)
    {
        bool response = await _partidaTorneoApplication.Register(request);

        if (response == false) return BadRequest("No se ha podido agregar la partida");

        return Ok(response);
    }

    [HttpDelete]
    [Route("Eliminar-Partida/{idPartida}")]
    public async Task<IActionResult> DeletePartida(int idPartida)
    {
        bool response = await _partidaTorneoApplication.Delete(idPartida);

        if (response == false) return BadRequest("No se ha podido eliminar la partida");

        return Ok(response);
    }

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
