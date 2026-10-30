using AS.API.Extensions;
using AS.Application.DTOs.Email;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using AS.Utils.Constantes;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class EmailController(IEmailApplicacion emailApplicacion) : ControllerBase
{
    private readonly IEmailApplicacion _emailApplicacion = emailApplicacion;

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

    /// <summary>
    /// Un correo a todos los usuarios, solo para la administración. Con SoloAMi llega únicamente a quien lo envía,
    /// para verlo antes. Quien ha desactivado los avisos no lo recibe. Devuelve a cuántos va
    /// </summary>
    [HttpPost]
    [Route("Novedades")]
    public async Task<IActionResult> EnviarNovedades([FromBody] EmailNovedadesDTO request)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        // 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
        if (!ConstAdministracion.EsAdministrador(idSolicitante))
            return BadRequest("Solo la administración puede enviar correos a todos los usuarios.");
        if (string.IsNullOrWhiteSpace(request.Asunto) || string.IsNullOrWhiteSpace(request.Html))
            return BadRequest("Faltan el asunto o el texto del correo.");

        int destinatarios = await _emailApplicacion.SendEmailNovedadesAsync(
            request.Asunto, request.Html, request.SoloAMi ? idSolicitante : null);
        return Ok(destinatarios);
    }
}
