using AS.Application.DTOs.Lista;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Lista;
using AS.Domain.Models;
using AS.Infrastructure.DTOs.Lista;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ListaController(IListaApplication listaApplication) : ControllerBase
{
    private readonly IListaApplication _listaApplication = listaApplication;

    [HttpGet]
    public async Task<ActionResult<List<Lista>>> GetListas()
    {
        var listas = await _listaApplication.GetListas();
        return Ok(listas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ListaDTO>> GetListaById(int id)
    {
        ListaDTO lista = await _listaApplication.GetListaById(id);
        if (lista == null)
        {
            return NotFound();
        }
        return Ok(lista);
    }

    [HttpGet("Inscripcion/{idInscripcion}")]
    public async Task<ActionResult<Lista>> GetListaInscripcionById(int idInscripcion)
    {
        ListaViewDTO lista = await _listaApplication.GetListaInscripcionById(idInscripcion);
        if (lista == null)
        {
            return NotFound();
        }
        return Ok(lista);
    }

    [HttpGet("listas/{idInscripcion}")]
    public async Task<ActionResult<Lista>> GetListasByInscripcion(int idInscripcion)
    {
        List<ListaViewDTO> listas = await _listaApplication.GetListasByInscripcionAsync(idInscripcion);

        return listas == null ? (ActionResult<Lista>)NotFound() : (ActionResult<Lista>)Ok(listas);
    }

    [HttpGet("Torneo/{idTorneo}")]
    public async Task<ActionResult<List<Lista>>> GetListasByTorneo(int idTorneo)
    {
        var listas = await _listaApplication.GetListasByTorneo(idTorneo);
        return Ok(listas);
    }

    [HttpGet("User/{idUsuario}")]
    public async Task<ActionResult<List<Lista>>> GetListasByUser(int idUsuario)
    {
        var listas = await _listaApplication.GetListasByUser(idUsuario);
        return Ok(listas);
    }

    [HttpPost]
    public async Task<ActionResult<ResultRegisterListarDTO>> RegisterLista([FromBody] CreateListaTorneoDTO lista)
    {
        ResultRegisterListarDTO result = await _listaApplication.RegisterLista(lista);
        if (!result.Result) return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<bool>> UpdateLista(int id, [FromBody] UpdateListaDTO request)
    {
        if (id != request.IdLista) return BadRequest("ID mismatch.");

        bool result = await _listaApplication.UpdateLista(request);
        if (!result)
            return BadRequest("Unable to register the lista.");

        return Ok(result);
    }

    [HttpPut("estado")]
    public async Task<ActionResult<bool>> UpdateEstadoLista([FromBody] UpdateEstadoListaDTO request)
    {
        bool result = await _listaApplication.UpdateEstadoLista(request);
        if (!result)
            return BadRequest("No se ha podido cambiar el estado de la lista");

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Lista>> DeleteLista(int id)
    {
        Lista deletedLista = await _listaApplication.Delete(id);
        if (deletedLista == null)
        {
            return NotFound();
        }

        return Ok(deletedLista);
    }

    [HttpGet]
    [Route("Lista-Torneo/{idTorneo}/{idUsuario}")]
    public async Task<ActionResult>GetListaTorneo(int idTorneo, int idUsuario)
    {
        ListaTorneoRequestDTO listaTorneoRequestDTO = new()
        {
            IdTorneo = idTorneo,
            IdUsuario = idUsuario
        };

        ListaDTO lista = await _listaApplication.GetListaTorneo(listaTorneoRequestDTO);
        if (lista is null)
        {
            return BadRequest("Unable to get the lista.");
        }
        return Ok(lista);
    }
}
