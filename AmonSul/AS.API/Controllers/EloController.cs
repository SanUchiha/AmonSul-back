using AS.Application.DTOs.Elo;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hangfire;

namespace AS.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class EloController(IEloApplication EloApplication) : ControllerBase
{
    private readonly IEloApplication _eloApplication = EloApplication;

    [HttpGet]
    [Route("usuario/{idUsuario}")]
    public async Task<IActionResult> GetPartidasAmistosas(int idUsuario)
    {
        ViewEloDTO response = await _eloApplication.GetEloByIdUsuarioAsync(idUsuario);
        
        if(response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetAllElos()
    {
        var response = await _eloApplication.GetAllElos();

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("Clasificacion")]
    public async Task<IActionResult> GetClasificacion()
    {
        List<ClasificacionEloDTO> response = 
            await _eloApplication.GetEloClasificacionAsync();

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("clasificacion-cache")]
    public async Task<IActionResult> GetClasificacionCache()
    {
        try
        {
            List<ClasificacionEloDTO> response = 
                await _eloApplication.GetClasificacionEloCacheAsync();

            if (response == null || response.Count == 0) 
            {
                return Ok(new { 
                    message = "No hay datos en caché disponibles",
                    data = new List<ClasificacionEloDTO>(),
                    cached = true,
                    count = 0
                });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { 
                    message = "Error al obtener la clasificación desde caché",
                    error = ex.Message,
                    cached = false,
                    success = false
                });
        }
    }
    
    [HttpGet]
    [Route("Ranking/{idUsuario}")]
    public async Task<IActionResult> GetRanking(int idUsuario)
    {
        int? ranking = await _eloApplication.GetRanking(idUsuario);

        return Ok(ranking);
    }

    [HttpGet]
    [Route("Mensual")]
    public async Task<IActionResult> GetClasificacionMensual()
    {
        List<ClasificacionEloDTO> response = await _eloApplication.GetClasificacionMensual();

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> RegistrarElo([FromBody] CreateEloDTO request)
    {
        var response = await _eloApplication.RegisterElo(request);

        if (!response) return BadRequest("No se ha podido registrar el elo");

        return StatusCode(StatusCodes.Status201Created, "El elo ha sido registrado con éxito");
    }

    [HttpPost]
    [Route("actualizar-cache")]
    public IActionResult ActualizarCache()
    {
        try
        {
            // Crear un job de Hangfire para actualizar el caché
            var jobId = BackgroundJob.Enqueue(() => _eloApplication.UpdateClasificacionEloCacheAsync());
            
            return Ok(new { 
                message = "Actualización del caché programada correctamente",
                status = "enqueued",
                success = true
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { 
                    message = "Error al programar la actualización del caché",
                    error = ex.Message,
                    status = "error",
                    success = false
                });
        }
    }
}
