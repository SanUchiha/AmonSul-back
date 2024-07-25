using AS.Application.DTOs.Torneo;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AS.API.Controllers
{
    [Route("api/[controller]")]
    //[Authorize]
    [ApiController]
    public class TorneoController : ControllerBase
    {
        private readonly ITorneoApplication _torneoApplication;

        public TorneoController(ITorneoApplication torneoApplication)
        {
            _torneoApplication = torneoApplication;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetTorneos()
        {
            var response = await _torneoApplication.GetTorneos();
            
            if(response == null) return NotFound();

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
    }
}
