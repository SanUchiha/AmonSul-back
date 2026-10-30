using AS.API.Extensions;
using AS.Application.DTOs.CircuitoTorneo;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Utils.Constantes;
using System.Globalization;
using System.Text;
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

        // Renombrar o borrar un circuito, solo quien lo creó desde la web. Los que no tienen creador, nadie
        private async Task<IActionResult?> SoloCreadorAsync(int idCircuito)
        {
            if (!User.TryGetIdUsuario(out int idSolicitante))
                return Unauthorized();

            // 400 y no 404: el interceptor del front manda al login con cualquier 404
            Circuito? circuito = await _circuitoApplication.GetCircuitoByIdAsync(idCircuito);
            if (circuito is null)
                return BadRequest("El circuito no existe.");
            if (circuito.IdUsuario != idSolicitante)
                return BadRequest("Solo quien creó el circuito puede cambiarlo o borrarlo.");

            return null;
        }

        private const int LONGITUD_MAXIMA_NOMBRE = 100;

        // «Amon Sûl …» lo crea la web cada año y recoge los torneos nuevos: un usuario no puede quitarle el nombre
        private static bool EsNombreReservado(string nombre) =>
            string.Concat(nombre.Normalize(NormalizationForm.FormD)
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
                .StartsWith("amon sul", StringComparison.OrdinalIgnoreCase);

        private static string? NombreNoValido(string nombre) =>
            nombre.Length is < 3 or > LONGITUD_MAXIMA_NOMBRE
                ? $"El nombre del circuito tiene que tener entre 3 y {LONGITUD_MAXIMA_NOMBRE} caracteres."
                : EsNombreReservado(nombre)
                    ? "Los circuitos Amon Sûl los crea la web cada año: elige otro nombre."
                    : null;

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

        /// <summary>
        /// Cualquiera puede crear un circuito: queda a su nombre
        /// </summary>
        [HttpPost("nuevo")]
        public async Task<IActionResult> CrearCircuito([FromBody] NombreCircuitoDTO request)
        {
            if (!User.TryGetIdUsuario(out int idSolicitante))
                return Unauthorized();

            string nombre = (request.NombreCircuito ?? "").Trim();
            if (NombreNoValido(nombre) is string error)
                return BadRequest(error);

            Circuito? creado = await _circuitoApplication.CrearCircuitoAsync(nombre, idSolicitante);
            if (creado is null)
                return BadRequest("Ya hay un circuito con ese nombre.");

            return Ok(creado);
        }

        [HttpPut("{idCircuito}")]
        public async Task<IActionResult> RenombrarCircuito(int idCircuito, [FromBody] NombreCircuitoDTO request)
        {
            if (await SoloCreadorAsync(idCircuito) is IActionResult denegado)
                return denegado;

            string nombre = (request.NombreCircuito ?? "").Trim();
            if (NombreNoValido(nombre) is string error)
                return BadRequest(error);

            if (!await _circuitoApplication.RenombrarCircuitoAsync(idCircuito, nombre))
                return BadRequest("Ya hay un circuito con ese nombre.");

            return Ok(true);
        }

        [HttpDelete("{idCircuito}")]
        public async Task<IActionResult> BorrarCircuito(int idCircuito)
        {
            if (await SoloCreadorAsync(idCircuito) is IActionResult denegado)
                return denegado;

            return Ok(await _circuitoApplication.BorrarCircuitoAsync(idCircuito));
        }

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
