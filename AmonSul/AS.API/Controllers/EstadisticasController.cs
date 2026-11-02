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

    private static readonly string[] Tramos = ["hasta-550", "600-650", "700-750", "800+"];
    private static readonly string[] Bandos = ["good", "evil"];

    // Los filtros que no se entienden se rechazan con su motivo, en vez de ignorarlos en silencio
    private static string? ErrorDeFiltro(FiltroEstadisticasDTO filtro)
    {
        if (!string.IsNullOrEmpty(filtro.Tramo) && !Tramos.Contains(filtro.Tramo))
            return "El tramo de puntos no existe.";
        if (!string.IsNullOrEmpty(filtro.Bando) && !Bandos.Contains(filtro.Bando))
            return "El bando debe ser good o evil.";
        return null;
    }

    /// <summary>
    /// Los ejércitos con al menos «minimo» partidas, del mejor porcentaje al peor
    /// </summary>
    [HttpGet("ejercitos")]
    public async Task<ActionResult<List<EjercitoBalanceDTO>>> GetRankingEjercitos(
        [FromQuery] FiltroEstadisticasDTO filtro,
        [FromQuery] int minimo = 10
    )
    {
        if (ErrorDeFiltro(filtro) is string error)
            return BadRequest(new { message = error });
        return Ok(await _estadisticasApplication.GetRankingEjercitosAsync(filtro, Math.Max(1, minimo)));
    }

    /// <summary>
    /// Todo de un ejército: balance, por puntos y temporada, rivales y jugadores
    /// </summary>
    [HttpGet("ejercitos/ficha")]
    public async Task<ActionResult<FichaEjercitoDTO>> GetFichaEjercito(
        [FromQuery] string ejercito,
        [FromQuery] FiltroEstadisticasDTO filtro
    )
    {
        if (ErrorDeFiltro(filtro) is string error)
            return BadRequest(new { message = error });
        FichaEjercitoDTO? ficha = await _estadisticasApplication.GetFichaEjercitoAsync(ejercito, filtro);
        return ficha is null ? BadRequest(new { message = "No hay partidas con ese ejército." }) : Ok(ficha);
    }

    /// <summary>
    /// Un ejército contra otro o contra un bando
    /// </summary>
    [HttpGet("ejercitos/cruce")]
    public async Task<ActionResult<CruceEjercitosDTO>> GetCruce(
        [FromQuery] string ejercito,
        [FromQuery] string? rival,
        [FromQuery] string? bandoRival,
        [FromQuery] FiltroEstadisticasDTO filtro
    )
    {
        if (ErrorDeFiltro(filtro) is string error)
            return BadRequest(new { message = error });
        if (string.IsNullOrWhiteSpace(rival) == string.IsNullOrWhiteSpace(bandoRival))
            return BadRequest(new { message = "Elige un ejército rival o un bando rival." });
        if (!string.IsNullOrWhiteSpace(bandoRival) && !Bandos.Contains(bandoRival))
            return BadRequest(new { message = "El bando rival debe ser good o evil." });

        CruceEjercitosDTO? cruce = await _estadisticasApplication.GetCruceAsync(
            ejercito, string.IsNullOrWhiteSpace(rival) ? null : rival, string.IsNullOrWhiteSpace(bandoRival) ? null : bandoRival, filtro);
        return cruce is null ? BadRequest(new { message = "No hay partidas con ese ejército." }) : Ok(cruce);
    }

    /// <summary>
    /// Los cruces más jugados y los más desequilibrados
    /// </summary>
    [HttpGet("ejercitos/cruces-destacados")]
    public async Task<ActionResult<CrucesDestacadosDTO>> GetCrucesDestacados([FromQuery] FiltroEstadisticasDTO filtro)
    {
        if (ErrorDeFiltro(filtro) is string error)
            return BadRequest(new { message = error });
        return Ok(await _estadisticasApplication.GetCrucesDestacadosAsync(filtro));
    }

    /// <summary>
    /// Los ejércitos de un jugador y contra cuáles le cuesta más
    /// </summary>
    [HttpGet("ejercitos/jugador/{idUsuario:int}")]
    public async Task<ActionResult<EjercitosJugadorDTO>> GetEjercitosJugador(
        int idUsuario,
        [FromQuery] FiltroEstadisticasDTO filtro
    )
    {
        if (ErrorDeFiltro(filtro) is string error)
            return BadRequest(new { message = error });
        return Ok(await _estadisticasApplication.GetEjercitosJugadorAsync(idUsuario, filtro));
    }
}
