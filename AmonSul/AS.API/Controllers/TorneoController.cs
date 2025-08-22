using AS.API.Filters;
using AS.Application.DTOs.PartidaTorneo;
using AS.Application.DTOs.Torneo;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Equipo;
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
    IGanadorApplication ganadorApplication,
    IInscripcionApplication inscripcionApplication) : ControllerBase
{
    private readonly ITorneoApplication _torneoApplication = torneoApplication;
    private readonly IInscripcionApplication _inscripcionApplication = inscripcionApplication;
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

    [HttpPost]
    [Route("Gestion/Generar-Ronda-Equipos")]
    public async Task<IActionResult> GenerarRondaEquipos([FromBody, Required] GenerarRondaEquiposDTO request)
    {
        bool response = await _partidaTorneoApplication.GenerateRoundEquipos(request);

        if (response == false) return BadRequest("No se ha podido generar la ronda");

        return Created(string.Empty, "La ronda ha sido creada con éxito");
    }

    [HttpPost]
    [Route("Gestion/Generar-Otra-Ronda-Equipos")]
    public async Task<IActionResult> GenerarOtraRondaEquipos([FromBody, Required] GenerarOtraRondaEquiposRequestDTO request)
    {
        bool response = await _partidaTorneoApplication.GenerarOtraRondaEquiposAsync(request);

        if (response == false) return BadRequest("No se ha podido generar la ronda");

        return Created(string.Empty, "La ronda ha sido creada con éxito");
    }

    [HttpPost]
    [Route("Gestion/Equipos/Modificar-Pairing-Equipos/{idTorneo}")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> ModificarPairingEquipos(
        [FromBody, Required] ModificarPairingTorneoEquiposDTO request, 
        int idTorneo)
    {
        bool response = await _partidaTorneoApplication.ModificarPairingEquiposAsync(request, idTorneo);

        if (response == false) return BadRequest("No se ha podido modificar el emparejamiento");

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
    [Route("Gestion/info-torneo-mas/{idTorneo}")]
    public async Task<IActionResult> GetInfoTorneoCreadoMas(int idTorneo)
    {
        TorneoGestionInfoMasDTO response = await _torneoApplication.GetInfoTorneoCreadoMasAsync(idTorneo);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("Gestion/info-torneo-equipo/{idTorneo}")]
    public async Task<IActionResult> GetInfoTorneoEquipoCreado(int idTorneo)
    {
        TorneoEquipoGestionInfoDTO torneo = await _torneoApplication.GetInfoTorneoEquipoCreado(idTorneo);
        if (torneo == null) return NotFound();

        return Ok(torneo);
    }

    [HttpGet]
    [Route("Gestion/Partidas/{idTorneo}")]
    public async Task<IActionResult> GetPartidasTorneo(int idTorneo)
    {
        List<PartidaTorneoDTO> response = await _partidaTorneoApplication.GetPartidasTorneoAsync(idTorneo);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("Gestion/Mas/Partidas/{idTorneo}")]
    public async Task<IActionResult> GetPartidasTorneoMas(int idTorneo)
    {
        List<PartidaTorneoMasDTO> response = await _partidaTorneoApplication.GetPartidasMasTorneoAsync(idTorneo);

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
    [Route("Gestion/subir-bases/{idTorneo}")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> UpdateBasesTorneo(
        [FromBody, Required] UpdateBasesDTO request, int idTorneo) =>
            Ok(await _torneoApplication.UpdateBasesTorneoAsync(request, idTorneo));

    [HttpPatch]
    [Route("Gestion/Handler-Listas/{idTorneo}")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> HandlerMostrarListas(
       [FromBody, Required] HandlerMostrarListasDTO request, int idTorneo) =>
           Ok(await _torneoApplication.HandlerMostrarListasAsync(request, idTorneo));

    [HttpPatch]
    [Route("Gestion/Handler-Clasificacion/{idTorneo}")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> HandlerMostrarClasificacion(
       [FromBody, Required] HandlerMostrarClasificacionDTO request, int idTorneo) =>
           Ok(await _torneoApplication.HandlerMostrarClasificacionAsync(request, idTorneo));

    [HttpGet]
    [Route("Gestion/Equipos/Equipos-disponibles/{idTorneo}")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> GetEquiposDisponibles(int idTorneo) 
    {
        List<EquipoDisponibleDTO> equipos = await _torneoApplication.GetEquiposDisponiblesAsync(idTorneo);

        return Ok(equipos);
    }
          

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
        bool response = await _partidaTorneoApplication.EdtarPairingAsync(request);

        if (response == false) return BadRequest("No se ha podido editar la partida");

        return Ok(response);
    }

    [HttpPut]
    [Route("Editar-Pairing-Equipos")]
    public async Task<IActionResult> EditarPairingEquipos([FromBody, Required] UpdatePairingTorneoDTO request)
    {
        bool response = await _partidaTorneoApplication.EdtarPairingEquiposAsync(request);

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

    [HttpGet]
    [Route("Partidas/Usuario/{idUsuario}/Torneo/{idTorneo}")]
    public async Task<IActionResult> GetPartidasUsuarioTorneo(int idUsuario, int idTorneo)
    {
        List<ViewPartidaTorneoDTO> response =
            await _partidaTorneoApplication.GetPartidasTorneoByUsuarioAsync(idTorneo, idUsuario);;

        return Ok(response);
    }


    [HttpGet]
    [Route("equipos/{idTorneo}")]
    public async Task<IActionResult> GetEquiposByTorneoAsync(int idTorneo)
    {
        List<EquipoDTO> equipos = await _inscripcionApplication.GetInscripcionesEquipoByTorneoAsync(idTorneo);

        if (equipos == null) return NotFound();

        return Ok(equipos);
    }
}
