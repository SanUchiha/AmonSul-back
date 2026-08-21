using AS.Application.DTOs.Estadisticas;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EstadisticasController(IEstadisticasApplication estadisticasApplication) : ControllerBase
{
    private readonly IEstadisticasApplication _estadisticasApplication = estadisticasApplication;

    /// <summary>
    /// Devuelve el top 3 y bottom 3 de ejércitos por win rate agrupados por bando (bien/oscuridad).
    /// </summary>
    [HttpGet("ejercito/top-bandos")]
    public async Task<ActionResult<TopBandosResponseDTO>> GetTopEjercitosPorBando([FromQuery] int top = 3)
    {
        if (top < 1 || top > 999)
            return BadRequest("El parámetro 'top' debe estar entre 1 y 999.");

        var result = await _estadisticasApplication.GetTopEjercitosPorBandoAsync(top);
        return Ok(result);
    }

    /// <summary>
    /// Devuelve el rating (estadísticas) de un ejército con filtros opcionales:
    /// fecha, ejércitos rivales concretos o bando rival (bien/oscuridad).
    /// </summary>
    [HttpPost("ejercito/rating")]
    public async Task<ActionResult<RatingEjercitoResponseDTO>> GetRatingEjercito(
        [FromBody] RatingEjercitoRequestDTO request
    )
    {
        if (string.IsNullOrWhiteSpace(request.Ejercito))
            return BadRequest("El campo 'ejercito' es obligatorio.");

        var result = await _estadisticasApplication.GetRatingEjercitoAsync(request);
        return Ok(result);
    }
}
