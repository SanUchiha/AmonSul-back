using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.Torneo;
using AS.Application.Interfaces;
using AS.Application.Services;
using AS.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Net;

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
        [Route("bases/{nombre}")]
        public async Task<IActionResult> GetBasesTorneo(string nombre)
        {
            try
            {
                var (fileBytes, fileName) = await _torneoApplication.GetBasesTorneo(nombre);

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
