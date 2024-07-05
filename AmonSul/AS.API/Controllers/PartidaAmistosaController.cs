using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AS.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class PartidaAmistosaController(IPartidaAmistosaApplication partidaAmistosaApplication) : ControllerBase
{
    private readonly IPartidaAmistosaApplication _partidaAmistosaApplication = partidaAmistosaApplication;

    [HttpGet]
    [Route("Todas")]
    [ProducesResponseType(typeof(List<ViewPartidaAmistosaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartidasAmistosas()
    {
        var response = await _partidaAmistosaApplication.GetPartidasAmistosas();
        
        if(response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("Validadas")]
    [ProducesResponseType(typeof(List<ViewPartidaAmistosaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartidasAmistosasValidadas()
    {
        var response = await _partidaAmistosaApplication.GetPartidasAmistosasValidadas();

        if (response == null) return NotFound();

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

    [HttpPut]
    [Route("Validar")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidadPartidaAmistosa([FromBody, Required] ValidarPartidaDTO request)
    {
        var response = await _partidaAmistosaApplication.ValidarPartidaAmistosa(request);

        if (response == false) return BadRequest("No se ha podido validar la partida");

        return Ok(response);
    }

    [HttpGet]
    [Route("{email}")]
    [ProducesResponseType(typeof(List<ViewPartidaAmistosaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartidaAmistosasByUsuario(string email)
    {
        List<ViewPartidaAmistosaDTO> response = await _partidaAmistosaApplication.GetPartidaAmistosasByUsuario(email);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("Validadas/{email}")]
    [ProducesResponseType(typeof(List<ViewPartidaAmistosaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartidaAmistosasByUsuarioValidadas(string email)
    {
        List<ViewPartidaAmistosaDTO> response = await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(email);

        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [Route("Pendientes/{email}")]
    [ProducesResponseType(typeof(List<ViewPartidaAmistosaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPartidaAmistosasByUsuarioPendientes(string email)
    {
        List<ViewPartidaAmistosaDTO> response = await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioPendientes(email);

        if (response == null) return NotFound();

        return Ok(response);
    }

}
