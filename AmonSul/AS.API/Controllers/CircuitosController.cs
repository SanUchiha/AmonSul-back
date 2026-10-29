using AS.Application.DTOs.CircuitoTorneo;
using AS.Application.Interfaces;
using AS.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CircuitosController(ICircuitoApplication circuitoApplication) : ControllerBase
    {
        private readonly ICircuitoApplication _circuitoApplication = circuitoApplication;

        [HttpGet]
        public async Task<List<Circuito>> Get() => await _circuitoApplication.GetAllCircuitosAsync();

        [HttpGet("{idCircuito}")]
        public async Task<Circuito?> Get(int idCircuito)
        {
            return await _circuitoApplication.GetCircuitoByIdAsync(idCircuito);
        }

        [HttpGet("{id}/torneos")]
        public async Task<IActionResult> GetTorneosByCircuito(int id)
        {
            List<CircuitoTorneo>? torneos = 
                await _circuitoApplication.GetTorneosByIdCircuitoAsync(id);
            
            if (torneos is null) 
                return NotFound("No se encontraron torneos asociados a este circuito.");

            var result = torneos.Select(lt => new
            {
                lt.IdCircuito,
                CircuitoNombre = lt.Circuito?.NombreCircuito,
                lt.IdTorneo,
                TorneoNombre = lt.Torneo?.NombreTorneo
            });

            return Ok(result);
        }

        [HttpGet("no-circuito-torneo/{idTorneo}")]
        public async Task<IActionResult> GetCircuitosNoTorneo(int idTorneo)
        {
            List<Circuito>? circuitos =
                await _circuitoApplication.GetCircuitosNoTorneoAsync(idTorneo);

            if (circuitos is null)
                return NotFound("No se encontraron circuitos que no estén asociados al torneo.");

            return Ok(circuitos);
        }

        [HttpGet("torneo/{idTorneo}")]
        public async Task<IActionResult> getCircuitosAsociadosATorneo(int idTorneo) => 
            Ok(await _circuitoApplication.GetCircuitosAsociadosATorneoAsync(idTorneo));

        [HttpPost]
        public async Task<IActionResult> AddTorneoToCircuito([FromBody] CircuitoTorneoDTO circuitoTorneoDTO) =>
            Ok(await _circuitoApplication.AddTorneoToCircuitoAsync(circuitoTorneoDTO));

        [HttpDelete("{idCircuito}/torneo/{idTorneo}")]
        public async Task<IActionResult> DeleteCircuitoTorneo(int idCircuito, int idTorneo) =>
            Ok(await _circuitoApplication.DeleteCircuitoTorneoAsync(idCircuito, idTorneo));
    }
}
