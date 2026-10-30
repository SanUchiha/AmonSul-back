using AS.API.Extensions;
using AS.Application.DTOs.CircuitoTorneo;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Utils.Constantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CircuitosController(
        ICircuitoApplication circuitoApplication,
        ITorneoApplication torneoApplication
    ) : ControllerBase
    {
        private readonly ICircuitoApplication _circuitoApplication = circuitoApplication;
        private readonly ITorneoApplication _torneoApplication = torneoApplication;

        /// <summary>
        /// Meter o sacar un torneo de un circuito cambia su clasificación: solo el organizador del torneo o la
        /// administración
        /// </summary>
        private async Task<IActionResult?> SoloOrganizadorAsync(int idTorneo)
        {
            if (!User.TryGetIdUsuario(out int idSolicitante))
                return Unauthorized();

            // 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
            if (!ConstAdministracion.EsAdministrador(idSolicitante)
                && !await _torneoApplication.EsOrganizadorAsync(idTorneo, idSolicitante))
                return BadRequest("Solo el organizador del torneo puede cambiar sus circuitos.");

            return null;
        }

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
        public async Task<IActionResult> AddTorneoToCircuito([FromBody] CircuitoTorneoDTO circuitoTorneoDTO)
        {
            if (await SoloOrganizadorAsync(circuitoTorneoDTO.IdTorneo) is IActionResult denegado)
                return denegado;

            return Ok(await _circuitoApplication.AddTorneoToCircuitoAsync(circuitoTorneoDTO));
        }

        [HttpDelete("{idCircuito}/torneo/{idTorneo}")]
        public async Task<IActionResult> DeleteCircuitoTorneo(int idCircuito, int idTorneo)
        {
            if (await SoloOrganizadorAsync(idTorneo) is IActionResult denegado)
                return denegado;

            return Ok(await _circuitoApplication.DeleteCircuitoTorneoAsync(idCircuito, idTorneo));
        }
    }
}
