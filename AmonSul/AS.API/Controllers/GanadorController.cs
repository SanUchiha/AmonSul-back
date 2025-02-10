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
    public async Task<IActionResult> SaveResult([FromBody] GuardarResultadosDTO guardarResultadosDTO)
    {
        if (guardarResultadosDTO.GanadoresDTO.Count == 0) return BadRequest();

        try
        {
            bool result = 
                await _ganadorApplication.SaveResultTournamentAsync(guardarResultadosDTO);
            if (!result)
                return BadRequest("No se pudo registrar los resultados del torneo.");

            return StatusCode(200);

        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ocurrió un problema al registrar los resultados: {ex.Message}");
        }
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
