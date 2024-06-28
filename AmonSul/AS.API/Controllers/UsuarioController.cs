using AS.Application.DTOs.Usuario;
using AS.Application.Interfaces;
using AS.Domain.Exceptions;
using AS.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]

    public class UsuarioController(IUsuarioApplication usuarioApplication) : ControllerBase
    {
        private readonly IUsuarioApplication _usuarioApplication = usuarioApplication;

        [HttpPost]
        [AllowAnonymous]
        [Route("Registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioDTO registrarUsuarioDTO)
        {
            try
            {
                var response = await _usuarioApplication.Register(registrarUsuarioDTO);
                
                if (response.Status) return Ok(response);
                return BadRequest("No se pudo registrar el usuario.");
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

        [HttpGet]
        [AllowAnonymous]

        [Route("All")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var response = await _usuarioApplication.GetAll();

                if (response is null) return NoContent();

                return Ok(response);
            }
            catch 
            {
                return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
            }
        }

        [HttpGet]
        [AllowAnonymous]

        [Route("Email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            try
            {
                var response = await _usuarioApplication.GetByEmail(email);

                if (response is null) return NotFound();

                return Ok(response);
            }
            catch
            {
                return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
            }
        }

        [HttpGet]
        [AllowAnonymous]

        [Route("Nick/{nick}")]
        public async Task<IActionResult> GetByNick(string nick)
        {
            try
            {
                var response = await _usuarioApplication.GetByNick(nick);

                if (response is null) return NotFound();

                return Ok(response);
            }
            catch
            {
                return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
            }
        }

        [HttpDelete]
        [AllowAnonymous]

        [Route("{email}")]
        public async Task<IActionResult> Delete(string email)
        {
            try
            {
                var response = await _usuarioApplication.Delete(email);

                if (!response) return NotFound();

                return Ok(response);
            }
            catch
            {
                return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
            }
        }

        [HttpPut]
        [AllowAnonymous]

        [Route("editar")]
        public async Task<IActionResult> Delete([FromBody] EditarUsuarioDTO request)
        {
            try
            {
                var response = await _usuarioApplication.Edit(request);

                if (!response) return NotFound();

                return Ok(response);
            }
            catch
            {
                return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
            }
        }

        [HttpGet]
        [AllowAnonymous]

        [Route("{email}")]
        public async Task<IActionResult> Get(string email)
        {
            try
            {
                var response = await _usuarioApplication.GetUsuario(email);

                if (response is null) return NotFound();

                return Ok(response);
            }
            catch
            {
                return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
            }
        }
    }
}
