using AS.API.Extensions;
using AS.Application.DTOs.Ganador;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class GanadorController(IGanadorApplication ganadorApplication) : ControllerBase
{
    private readonly IGanadorApplication _ganadorApplication = ganadorApplication;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            List<GanadorDTO> ganadores = await _ganadorApplication.GetAll();
            return Ok(ganadores);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ocurrió un problema al obtener los ganadores: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var ganador = await _ganadorApplication.GetById(id);
            if (ganador == null)
            {
                return NotFound($"No se encontró un ganador con ID {id}.");
            }
            return Ok(ganador);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ocurrió un problema al obtener el ganador: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveResult(
        [FromBody] GuardarResultadosDTO guardarResultadosDTO)
    {
        if (guardarResultadosDTO.GanadoresDTO.Count <= 0) return BadRequest();

        if (!User.TryGetIdUsuario(out int idUsuarioSolicitante))
            return Unauthorized();

        int idTorneo = guardarResultadosDTO.GenerarRondaDTO.IdTorneo;

        // Todas las posiciones tienen que ser del torneo que se guarda
        if (guardarResultadosDTO.GanadoresDTO.Any(g => g.IdTorneo != idTorneo))
            return BadRequest("Los resultados no corresponden a este torneo.");

        // 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
        if (!await _ganadorApplication.PuedeGuardarResultadosAsync(idTorneo, idUsuarioSolicitante))
            return BadRequest("Solo el organizador del torneo puede guardar sus resultados.");

        // Guardar dos veces duplicaría las posiciones y aplicaría el ELO del torneo dos veces
        if (await _ganadorApplication.IsSave(idTorneo))
            return BadRequest("Los resultados de este torneo ya están guardados.");

        // False si otra petición los guardó a la vez, después de la comprobación anterior
        bool result = await _ganadorApplication.SaveResultTournamentAsync(guardarResultadosDTO);
        if (!result) return BadRequest("Los resultados de este torneo ya están guardados.");

        return StatusCode(200);

    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            bool result = await _ganadorApplication.Delete(id);
            if (result)
            {
                return NoContent(); // Eliminación exitosa
            }
            return NotFound($"No se encontró un ganador con ID {id}.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ocurrió un problema al eliminar el ganador: {ex.Message}");
        }
    }
}
