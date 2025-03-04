using AS.Application.DTOs.Elo;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class EloController(IEloApplication EloApplication) : ControllerBase
{
    private readonly IEloApplication _eloApplication = EloApplication;

    [HttpGet]
    [Route("usuario/{email}")]
    public async Task<IActionResult> GetPartidasAmistosas(string email)
    {
        ViewEloDTO response = await _eloApplication.GetElo(email);
        
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
            await _eloApplication.GetClasificacion();

        if (response == null) return NotFound();

        return Ok(response);
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
}
