using AS.Application.DTOs;
using AS.Application.Interfaces;
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
        public async Task<IActionResult> Registrar(RegistrarUsuarioDTO registrarUsuarioDTO)
        {
            try
            {
                registrarUsuarioDTO.Contraseña = _utilidades.encriptarSHA256(registrarUsuarioDTO.Contraseña);
                var response = await _usuarioApplication.Register(registrarUsuarioDTO);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
    }
}
