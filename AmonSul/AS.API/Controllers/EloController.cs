using AS.Application.DTOs.Elo;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
    [ProducesResponseType(typeof(ViewEloDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartidasAmistosas(string email)
    {
        var response = await _eloApplication.GetElo(email);
        
        if(response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(List<ViewEloDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllElos()
    {
        var response = await _eloApplication.GetAllElos();

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("Clasificacion")]
    [ProducesResponseType(typeof(List<ClasificacionEloDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetClasificacion()
    {
        List<ClasificacionEloDTO> response = await _eloApplication.GetClasificacion();

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarElo([FromBody] CreateEloDTO request)
    {
        var response = await _eloApplication.RegisterElo(request);

        if (!response) return BadRequest("No se ha podido registrar el elo");

        return StatusCode(StatusCodes.Status201Created, "El elo ha sido registrado con éxito");
    }
}
