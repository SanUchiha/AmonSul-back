using AS.Application.DTOs.Faccion;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class FaccionController(IFaccionApplication faccionApplication) : ControllerBase
{
    private readonly IFaccionApplication _faccionApplication = faccionApplication;

    [HttpGet]
    [AllowAnonymous]
    [Route("")]
    public async Task<IActionResult> GetFacciones()
    {
        try
        {
            var response = await _faccionApplication.GetFacciones();
            return Ok(response);
        }
        catch (Exception ex) 
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [Route("Registrar")]
    public async Task<IActionResult> RegisterFaccion([FromBody] RegistrarFaccionDTO registrarFaccionDTO)
    {
        try 
        {
            var response = await _faccionApplication.Register(registrarFaccionDTO);

            if (!response) return BadRequest(response);
            return Ok(response);
        }
        catch (Exception ex) 
        {
            return BadRequest(ex.Message);
        }
        
    }
}
