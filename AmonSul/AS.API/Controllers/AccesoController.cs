using AS.Application.DTOs;
using AS.Domain.Models;
using AS.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AS.API.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class AccesoController : ControllerBase
    {
        private readonly DbamonsulContext _dbamonsulContext;
        private readonly Utilidades _utilidades;

        public AccesoController(DbamonsulContext dbamonsulContext, Utilidades utilidades)
        {
            _dbamonsulContext = dbamonsulContext;
            _utilidades = utilidades;
        }

        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> Registrar(Usuario usuario)
        {
            usuario.Contraseña = _utilidades.encriptarSHA256(usuario.Contraseña);

            await _dbamonsulContext.Usuarios.AddAsync(usuario);
            await _dbamonsulContext.SaveChangesAsync();

            if (usuario.IdUsuario != 0) return Ok(usuario);
            else return BadRequest();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var foundUser = await _dbamonsulContext.Usuarios.Where(u =>
                u.Email == loginDTO.Email &&
                u.Contraseña == _utilidades.encriptarSHA256(loginDTO.Password!))
                .FirstOrDefaultAsync();
            if (foundUser is null) return BadRequest();

            return Ok(new LoginResponse { Token = _utilidades.generarJWT(foundUser) });
        }

    }
}
