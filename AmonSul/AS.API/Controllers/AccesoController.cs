using AS.Application.Interfaces;
using AS.Infrastructure.DTOs.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers;

[Route("api/[controller]")]
[AllowAnonymous]
[ApiController]
public class AccesoController(
    ILoginApplication loginApplication) : ControllerBase
{
    private readonly ILoginApplication _loginApplication = loginApplication;

    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login(LoginDTO loginDTO)
    {
        if (loginDTO == null || loginDTO.Email == null || loginDTO.Password == null)
            return BadRequest();

        LoginResponse response = await _loginApplication.Login(loginDTO);

        return Ok(response);
    }

    // Cuando caduca el token, la app pide otro con el refresh token: con entrar una vez basta
    [HttpPost]
    [Route("Refresh")]
    public async Task<IActionResult> Refresh(RefreshDTO refreshDTO)
    {
        if (string.IsNullOrEmpty(refreshDTO?.RefreshToken))
            return BadRequest();

        LoginResponse response = await _loginApplication.Refresh(refreshDTO.RefreshToken);

        return response.IsAccess ? Ok(response) : Unauthorized();
    }

    // Con el refresh token basta: al cerrar sesión el token de acceso puede haber caducado ya
    [HttpPost]
    [Route("Logout")]
    public async Task<IActionResult> Logout(RefreshDTO refreshDTO)
    {
        if (string.IsNullOrEmpty(refreshDTO?.RefreshToken))
            return BadRequest();

        await _loginApplication.CerrarSesion(refreshDTO.RefreshToken);

        return NoContent();
    }
}
