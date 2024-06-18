using AS.Application.DTOs;
using AS.Application.Interfaces;
using AS.Domain.Exceptions;
using AS.Domain.Models;
using AS.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioApplication _usuarioApplication;
        private readonly Utilidades _utilidades;

        public UsuarioController(Utilidades utilidades, IUsuarioApplication usuarioApplication)
        {
            _utilidades = utilidades;
            _usuarioApplication = usuarioApplication;
        }

        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioDTO registrarUsuarioDTO)
        {
            try
            {
                bool response = await _usuarioApplication.Register(registrarUsuarioDTO);
                
                if (response)
                {
                    return Ok("Usuario registrado con éxito.");
                }
                else
                {
                    return BadRequest("No se pudo registrar el usuario.");
                }
            }
            catch (UniqueKeyViolationException ex)
            {
                return Conflict(new { message = ex.Message});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error en el servidor.", detail = ex.Message });
            }

        }
    }
}
