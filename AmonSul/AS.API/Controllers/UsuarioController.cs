using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.Usuario;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Usuario;
using AS.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers;

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

    [HttpPut]
    [Route("Cambiar-Pass")]
    public async Task<IActionResult> CambiarPass([FromBody] CambiarPassDTO cambiarPassDTO)
    {
        if (cambiarPassDTO == null || 
            string.IsNullOrWhiteSpace(cambiarPassDTO.OldPass) || 
            string.IsNullOrWhiteSpace(cambiarPassDTO.NewPass))
            return BadRequest(new { message = "Los datos proporcionados son inválidos." });

        try
        {
            bool response = await _usuarioApplication.CambiarPass(cambiarPassDTO);

            if (!response)
                return BadRequest(new { message = "No se pudo cambiar la contraseña. Verifica los datos ingresados." });

            return Ok(new { message = "Contraseña cambiada con éxito." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor.", detail = ex.Message });
        }
    }

    [HttpGet]
    [Route("All")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            List<ViewUsuarioPartidaDTO> response = await _usuarioApplication.GetAll();

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
    [Route("Recordar-Pass/{email}")]
    public async Task<IActionResult> RecordarPass(string email)
    {
        try
        {
            bool response = await _usuarioApplication.RecordarPass(email);

            if (!response) return NoContent();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetUsuarios()
    {
        try
        {
            List<UsuarioDTO> response = await _usuarioApplication.GetUsuarios();

            if (response is null) return NoContent();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }

    [HttpGet]
    [Route("torneo/{idTorneo}")]
    public async Task<IActionResult> GetUsuariosByTorneo(int idTorneo)
    {
        try
        {
            List<UsuarioInscripcionTorneoDTO> response = await _usuarioApplication.GetUsuariosByTorneo(idTorneo);

            if (response is null) return NoContent();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }

    [HttpGet]
    [Route("Nicks")]
    public async Task<IActionResult> GetNicks()
    {
        try
        {
            var response = await _usuarioApplication.GetNicks();

            if (response is null) return NoContent();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }

    [HttpGet]
    [Route("Data/{idUsuario}")]
    public async Task<IActionResult> GetUsuarioData(int idUsuario)
    {
        try
        {
            UsuarioDataDTO response = await _usuarioApplication.GetUsuarioDataAsync(idUsuario);
            if (response is null) return NoContent();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }

    [HttpGet]
    [Route("Email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        try
        {
            ViewUsuarioPartidaDTO? response = await _usuarioApplication.GetByEmail(email);

            if (response is null) return NotFound();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }

    [HttpGet]
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
    [Route("editar")]
    public async Task<IActionResult> Edit([FromBody] EditarUsuarioDTO request)
    {
        try
        {
            bool response = await _usuarioApplication.EditAsync(request);

            if (!response) return NotFound();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }


    [HttpGet]
    [Route("Proteccion-Datos/{idUsuario}")]
    public async Task<IActionResult> GetProteccionDatos(int idUsuario) =>
        Ok(await _usuarioApplication.GetProteccionDatos(idUsuario));
    
    [HttpPut]
    [Route("Proteccion-Datos")]
    public async Task<IActionResult> UpdateProteccionDatos([FromBody] UpdateProteccionDatosDTO request)
    {
        try
        {
            bool response = await _usuarioApplication.UpdateProteccionDatos(request);

            if (!response) return NotFound();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }

    [HttpPut]
    [Route("modificar-faccion")]
    public async Task<IActionResult> ModificarFaccion([FromBody] EditarFaccionUsuarioDTO request)
    {
        try
        {
            var response = await _usuarioApplication.ModificarFaccion(request);

            if (!response) return NotFound();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }

    [HttpGet]
    [Route("{email}")]
    public async Task<IActionResult> Get(string email)
    {
        try
        {
            UsuarioViewDTO? response = await _usuarioApplication.GetUsuario(email);

            if (response is null) return NotFound();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }

    [HttpGet]
    [Route("detalle/{email}")]
    public async Task<IActionResult> GetDetalle(string email)
    {
        try
        {
            var response = await _usuarioApplication.GetDetalleUsuarioByEmail(email);

            if (response is null) return NotFound();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }

    [HttpGet]
    [Route("Id/{idUsuario}")]
    public async Task<IActionResult> GetNickById(int idUsuario)
    {
        try
        {
            string response = await _usuarioApplication.GetNickById(idUsuario);

            if (response is null) return NotFound();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }

    [HttpGet]
    [Route("no-inscritos-torneo/{idTorneo}")]
    public async Task<IActionResult> GetUsuariosNoInscritosTorneo(int idTorneo)
    {
        try
        {
            List<UsuarioSinEquipoDTO> response = await _usuarioApplication.GetUsuariosNoInscritosTorneoAsync(idTorneo);

            if (response is null) return NoContent();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }
}