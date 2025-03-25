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
}
