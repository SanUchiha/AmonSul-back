using AS.API.Extensions;
using AS.Application.DTOs.Email;
using AS.Application.Interfaces;
using AS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using AS.Utils.Constantes;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class EmailController(
    IEmailApplicacion emailApplicacion,
    INotificacionPushApplication notificacionPushApplication) : ControllerBase
{
    private readonly IEmailApplicacion _emailApplicacion = emailApplicacion;
    private readonly INotificacionPushApplication _notificacionPushApplication = notificacionPushApplication;

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

        // Con SoloAMi, para verlo antes de mandarlo a todos, como el correo
        string resumen = string.IsNullOrWhiteSpace(request.Resumen) ? "Hay novedades en Amon Sûl." : request.Resumen;
        if (request.SoloAMi)
            await _notificacionPushApplication.EnviarAUsuariosAsync([idSolicitante], request.Asunto, resumen, "/home");
        else
            AvisoPush.ATodos(request.Asunto, resumen, "/home");

        int destinatarios = await _emailApplicacion.SendEmailNovedadesAsync(
            request.Asunto, request.Html, request.SoloAMi ? idSolicitante : null);
        return Ok(destinatarios);
    }

    /// <summary>
    /// El aviso de la política de privacidad, solo para la administración. Llega a todos, en tandas diarias por el
    /// límite de Gmail. Con SoloAMi llega únicamente a quien lo envía, para verlo antes. Devuelve a cuántos va
    /// </summary>
    [HttpPost]
    [Route("Aviso-Privacidad")]
    public async Task<IActionResult> EnviarAvisoPrivacidad([FromBody] EmailAvisoPrivacidadDTO request)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        // 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
        if (!ConstAdministracion.EsAdministrador(idSolicitante))
            return BadRequest("Solo la administración puede enviar correos a todos los usuarios.");

        int destinatarios = await _emailApplicacion.SendEmailAvisoPrivacidadAsync(
            request.SoloAMi ? idSolicitante : null);
        return Ok(destinatarios);
    }
}
