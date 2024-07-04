using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AS.API.Controllers;

[Route("[controller]")]
//[Authorize]
[ApiController]
public class PartidaAmistosaController(IPartidaAmistosaApplication partidaAmistosaApplication) : ControllerBase
{
    private readonly IPartidaAmistosaApplication _partidaAmistosaApplication = partidaAmistosaApplication;

    /// <summary>
    /// Obtener las partidas amistosas
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(List<ViewPartidaAmistosaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartidasAmistosas()
    {
        var response = await _partidaAmistosaApplication.GetPartidasAmistosas();
        
        if(response == null) return NotFound();

        return Ok(response);
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearPartidaAmistosa([FromBody, Required] CreatePartidaAmistosaDTO request)
    {
        var response = await _partidaAmistosaApplication.Register(request);

        if (response == false) return BadRequest("No se ha podido crear la partida");

        return Created();
    }

}
