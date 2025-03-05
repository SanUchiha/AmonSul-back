using AS.Application.DTOs.Lista;
using AS.Application.Interfaces;
using AS.Application.Services;
using AS.Domain.Models;
using AS.Infrastructure.DTOs.Lista;
using AS.Infrastructure.DTOs.Login;
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
    public async Task<ActionResult<IEnumerable<Lista>>> GetListas()
    {
        var listas = await _listaApplication.GetListas();
        return Ok(listas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Lista>> GetListaById(int id)
    {
        var lista = await _listaApplication.GetListaById(id);
        if (lista == null)
        {
            return NotFound();
        }
        return Ok(lista);
    }

    [HttpGet("inscripcion/{idInscripcion}")]
    public async Task<ActionResult<Lista>> GetListaInscripcionById(int idInscripcion)
    {
        var lista = await _listaApplication.GetListaInscripcionById(idInscripcion);
        if (lista == null)
        {
            return NotFound();
        }
        return Ok(lista);
    }

    [HttpGet("torneo/{idTorneo}")]
    public async Task<ActionResult<IEnumerable<Lista>>> GetListasByTorneo(int idTorneo)
    {
        var listas = await _listaApplication.GetListasByTorneo(idTorneo);
        return Ok(listas);
    }

    [HttpGet("user/{idUsuario}")]
    public async Task<ActionResult<IEnumerable<Lista>>> GetListasByUser(int idUsuario)
    {
        var listas = await _listaApplication.GetListasByUser(idUsuario);
        return Ok(listas);
    }

    [HttpPost]
    public async Task<ActionResult<bool>> RegisterLista([FromBody] CreateListaTorneoDTO lista)
    {
        bool result = await _listaApplication.RegisterLista(lista);
        if (!result)
            return BadRequest("Unable to register the lista.");

        return Ok(result);
    }

    //Actualiza una lista de un torneo    
    [HttpPut("{id}")]
    public async Task<ActionResult<bool>> UpdateLista(int id, [FromBody] UpdateListaDTO request)
    {
        if (id != request.IdLista)
            return BadRequest("ID mismatch.");

        bool result = await _listaApplication.UpdateLista(request);
        if (!result)
            return BadRequest("Unable to register the lista.");

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Lista>> DeleteLista(int id)
    {
        var deletedLista = await _listaApplication.Delete(id);
        if (deletedLista == null)
        {
            return NotFound();
        }

        return Ok(deletedLista);
    }

    
    [HttpGet]
    [Route("Lista-Torneo/{idTorneo}/{idUsuario}")]
    public async Task<ActionResult<bool>>GetListaTorneo(int idTorneo, int idUsuario)
    {
        ListaTorneoRequestDTO listaTorneoRequestDTO = new()
        {
            IdTorneo = idTorneo,
            IdUsuario = idUsuario
        };

        string? listaData = await _listaApplication.GetListaTorneo(listaTorneoRequestDTO);
        if (listaData is null)
        {
            return BadRequest("Unable to get the lista.");
        }
        return Ok(listaData);
    }
}
