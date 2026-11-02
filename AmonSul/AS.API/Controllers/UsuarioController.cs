using AS.API.Extensions;
using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.Usuario;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Usuario;
using AS.Domain.Exceptions;
using AS.Utils.Constantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AS.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class UsuarioController(IUsuarioApplication usuarioApplication, ITorneoApplication torneoApplication)
    : ControllerBase
{
    private readonly IUsuarioApplication _usuarioApplication = usuarioApplication;
    private readonly ITorneoApplication _torneoApplication = torneoApplication;

    // 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
    private const string SOLO_TUS_DATOS = "Solo puedes ver tus propios datos.";
    private const string SOLO_ADMINISTRACION = "Solo la administración puede ver los datos de todos los usuarios.";

    private bool EsAdministracion() =>
        User.TryGetIdUsuario(out int idSolicitante) && ConstAdministracion.EsAdministrador(idSolicitante);

    // Los datos personales de un usuario: solo para él mismo o la administración
    private bool EsUsuarioOAdministracion(int idUsuario) =>
        EsAdministracion() || (User.TryGetIdUsuario(out int idSolicitante) && idSolicitante == idUsuario);

    // Por correo, se compara con el de la sesión antes de buscar: así no se averigua qué correos están dados de alta
    private bool EsSuCorreoOAdministracion(string email) =>
        EsAdministracion()
        || string.Equals(User.FindFirst(ClaimTypes.Email)?.Value?.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase);

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
        if (!EsAdministracion()) return BadRequest(SOLO_ADMINISTRACION);

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
        // Lleva las listas de cada inscrito, también las ocultas: solo para quien organiza el torneo
        if (!EsAdministracion()
            && !(User.TryGetIdUsuario(out int idSolicitante) && await _torneoApplication.EsOrganizadorAsync(idTorneo, idSolicitante)))
            return BadRequest("Solo el organizador del torneo puede ver sus inscritos con las listas.");

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
        if (!EsAdministracion()) return BadRequest(SOLO_ADMINISTRACION);

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

            // La ficha la ve cualquiera; el correo, solo su dueño
            if (!EsUsuarioOAdministracion(idUsuario)) response.Email = null;

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
        if (!EsSuCorreoOAdministracion(email)) return BadRequest(SOLO_TUS_DATOS);

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
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        // Cada uno solo edita su perfil. 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
        if (idSolicitante != request.IdUsuario)
            return BadRequest("Solo puedes modificar tu propio perfil.");

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
        EsUsuarioOAdministracion(idUsuario)
            ? Ok(await _usuarioApplication.GetProteccionDatos(idUsuario))
            : BadRequest(SOLO_TUS_DATOS);
    
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
        if (!EsSuCorreoOAdministracion(email)) return BadRequest(SOLO_TUS_DATOS);

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
        if (!EsSuCorreoOAdministracion(email)) return BadRequest(SOLO_TUS_DATOS);

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

    [HttpGet]
    [Route("IdNick")]
    public async Task<IActionResult> GetAllIdNick()
    {
        try
        {
            var response = await _usuarioApplication.GetAllIdNickAsync();

            if (response is null || response.Count == 0) return NoContent();

            return Ok(response);
        }
        catch
        {
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }
}