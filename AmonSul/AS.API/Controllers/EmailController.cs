using AS.Application.DTOs.Email;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly IEmailApplicacion _emailApplicacion;

    public EmailController(IEmailApplicacion emailApplicacion)
    {
        _emailApplicacion = emailApplicacion;
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("Contacto")]
    public async Task<IActionResult> SendEmailContacto([FromBody] EmailContactoDTO emailContactoDTO)
    {
        try
        {
            await _emailApplicacion.SendEmailContacto(emailContactoDTO);

            return Ok("Consulta enviada");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }
}
