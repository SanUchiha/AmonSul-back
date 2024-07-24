using AS.Application.DTOs.Lista;
using AS.Application.Interfaces;
using AS.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ListaController : ControllerBase
    {
        private readonly IListaApplication _listaApplication;

        public ListaController(IListaApplication listaApplication)
        {
            _listaApplication = listaApplication;
        }

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
            var result = await _listaApplication.RegisterLista(lista);
            if (!result)
            {
                return BadRequest("Unable to register the lista.");
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Lista>> UpdateLista(int id, [FromBody] Lista lista)
        {
            if (id != lista.IdLista)
            {
                return BadRequest("ID mismatch.");
            }

            var updatedLista = await _listaApplication.UpdateLista(lista);
            if (updatedLista == null)
            {
                return NotFound();
            }

            return Ok(updatedLista);
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
    }
}
