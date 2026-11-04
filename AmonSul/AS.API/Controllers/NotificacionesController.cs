using AS.API.Extensions;
using AS.Application.DTOs.Notificaciones;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers;

/// <summary>
/// Los móviles de cada usuario, para mandarles notificaciones push
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificacionesController(INotificacionPushApplication notificacionPushApplication) : ControllerBase
{
    private const int LONGITUD_MAXIMA_TOKEN = 512;
    private static readonly string[] PLATAFORMAS = ["android", "ios"];

    private readonly INotificacionPushApplication _notificacionPushApplication = notificacionPushApplication;

    [HttpPost("Dispositivo")]
    public async Task<IActionResult> Registrar(DispositivoPushDTO dispositivo)
    {
        if (!User.TryGetIdUsuario(out int idUsuario))
            return Unauthorized();
        if (string.IsNullOrWhiteSpace(dispositivo.Token)
            || dispositivo.Token.Length > LONGITUD_MAXIMA_TOKEN
            || !PLATAFORMAS.Contains(dispositivo.Plataforma))
            return BadRequest();

        await _notificacionPushApplication.RegistrarDispositivoAsync(idUsuario, dispositivo.Token, dispositivo.Plataforma!);
        return Ok();
    }

    // Al cerrar sesión: ese móvil deja de recibir los avisos de la cuenta
    [HttpDelete("Dispositivo")]
    public async Task<IActionResult> Quitar(DispositivoPushDTO dispositivo)
    {
        if (!User.TryGetIdUsuario(out int idUsuario))
            return Unauthorized();
        if (string.IsNullOrWhiteSpace(dispositivo.Token))
            return BadRequest();

        await _notificacionPushApplication.QuitarDispositivoAsync(idUsuario, dispositivo.Token);
        return Ok();
    }

    // Para comprobar que llegan: solo a los móviles de quien lo pide
    [HttpPost("Prueba")]
    public async Task<IActionResult> Prueba()
    {
        if (!User.TryGetIdUsuario(out int idUsuario))
            return Unauthorized();

        return Ok(await _notificacionPushApplication.EnviarAUsuariosAsync(
            [idUsuario], "Amon Sûl", "Si ves este aviso, las notificaciones funcionan.", "/perfil-usuario"));
    }
}
