using AS.API.Extensions;
using AS.Application.DTOs.Lista;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Lista;
using AS.Domain.Models;
using AS.Infrastructure.DTOs.Lista;
using AS.Utils.Constantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ListaController(IListaApplication listaApplication, IInscripcionApplication inscripcionApplication)
    : ControllerBase
{
    private readonly IListaApplication _listaApplication = listaApplication;
    private readonly IInscripcionApplication _inscripcionApplication = inscripcionApplication;

    // 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
    private const string SOLO_SU_LISTA =
        "Solo el propio jugador, su capitán o el organizador pueden cambiar esta lista.";

    // Subir, cambiar o borrar la lista de una inscripción: el jugador, su capitán, el compañero de una pareja con
    // lista conjunta, el organizador o la administración
    private async Task<bool> PuedeTocarListaAsync(int idLista) =>
        User.TryGetIdUsuario(out int idSolicitante)
        && await _listaApplication.GetIdInscripcionDeListaAsync(idLista) is int idInscripcion
        && await _inscripcionApplication.PuedeTocarInscripcionAsync(idInscripcion, idSolicitante, companeroDeListaConjunta: true);

    [HttpGet("{id}")]
    public async Task<ActionResult<ListaDTO>> GetListaById(int id)
    {
        if (!User.TryGetIdUsuario(out int idUsuarioSolicitante))
            return Unauthorized();

        // 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
        if (!await _listaApplication.PuedeVerListaAsync(id, idUsuarioSolicitante))
            return BadRequest("Las listas de este torneo están ocultas.");

        ListaDTO lista = await _listaApplication.GetListaById(id);
        if (lista == null)
        {
            return NotFound();
        }
        return Ok(lista);
    }

    [HttpGet("Inscripcion/{idInscripcion}")]
    public async Task<ActionResult<Lista>> GetListaInscripcionById(int idInscripcion)
    {
        if (!User.TryGetIdUsuario(out int idUsuarioSolicitante))
            return Unauthorized();

        if (!await _listaApplication.PuedeVerListasInscripcionAsync(idInscripcion, idUsuarioSolicitante))
            return BadRequest("Las listas de este torneo están ocultas.");

        ListaViewDTO lista = await _listaApplication.GetListaInscripcionById(idInscripcion);
        if (lista == null)
        {
            return NotFound();
        }
        return Ok(lista);
    }

    [HttpGet("listas/{idInscripcion}")]
    public async Task<ActionResult<Lista>> GetListasByInscripcion(int idInscripcion)
    {
        if (!User.TryGetIdUsuario(out int idUsuarioSolicitante))
            return Unauthorized();

        if (!await _listaApplication.PuedeVerListasInscripcionAsync(idInscripcion, idUsuarioSolicitante))
            return BadRequest("Las listas de este torneo están ocultas.");

        List<ListaViewDTO> listas = await _listaApplication.GetListasByInscripcionAsync(idInscripcion);

        return listas == null ? (ActionResult<Lista>)NotFound() : (ActionResult<Lista>)Ok(listas);
    }

    [HttpPost]
    public async Task<ActionResult<ResultRegisterListarDTO>> RegisterLista([FromBody] CreateListaTorneoDTO lista)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();
        if (!await _inscripcionApplication.PuedeTocarInscripcionAsync(lista.IdInscripcion, idSolicitante, companeroDeListaConjunta: true))
            return BadRequest(SOLO_SU_LISTA);

        ResultRegisterListarDTO result = await _listaApplication.RegisterLista(lista);
        if (!result.Result) return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<bool>> UpdateLista(int id, [FromBody] UpdateListaDTO request)
    {
        if (id != request.IdLista) return BadRequest("ID mismatch.");
        if (!await PuedeTocarListaAsync(id)) return BadRequest(SOLO_SU_LISTA);

        (bool ok, string? error) = await _listaApplication.UpdateLista(request);
        if (!ok)
            return BadRequest(error ?? "Unable to register the lista.");

        return Ok(true);
    }

    [HttpPut("estado")]
    public async Task<ActionResult<bool>> UpdateEstadoLista([FromBody] UpdateEstadoListaDTO request)
    {
        // Validar una lista es cosa del organizador
        if (!User.TryGetIdUsuario(out int idSolicitante)
            || await _listaApplication.GetIdInscripcionDeListaAsync(request.IdLista) is not int idInscripcion
            || !await _inscripcionApplication.EsOrganizadorDeInscripcionAsync(idInscripcion, idSolicitante))
            return BadRequest("Solo el organizador puede cambiar el estado de la lista.");

        bool result = await _listaApplication.UpdateEstadoLista(request);
        if (!result)
            return BadRequest("No se ha podido cambiar el estado de la lista");

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Lista>> DeleteLista(int id)
    {
        if (!await PuedeTocarListaAsync(id)) return BadRequest(SOLO_SU_LISTA);

        Lista deletedLista = await _listaApplication.Delete(id);
        if (deletedLista == null)
        {
            return NotFound();
        }

        return Ok(deletedLista);
    }

    [AllowAnonymous]
    /// <summary>
    /// Lista de un lado de la partida (1 o 2): en una liga, la que llevaba cuando se registró el resultado
    /// </summary>
    [HttpGet("Partida/{idPartida}/{lado}")]
    public async Task<ActionResult> GetListaPartida(int idPartida, int lado)
    {
        if (!User.TryGetIdUsuario(out int idSolicitante))
            return Unauthorized();

        // 400 y no 404: el interceptor del front manda a iniciar sesión con cualquier 404
        (ListaDTO? lista, string? error) = await _listaApplication.GetListaPartidaAsync(idPartida, lado, idSolicitante);
        return lista is null ? BadRequest(error) : Ok(lista);
    }

    [HttpPost("migrar-cloudinary")]
    public async Task<ActionResult<MigracionCloudinaryResultDTO>> MigrarListasACloudinary()
    {
        // Tarea de mantenimiento: solo la administración
        if (!User.TryGetIdUsuario(out int idSolicitante) || !ConstAdministracion.EsAdministrador(idSolicitante))
            return BadRequest("Solo la administración puede lanzar la migración.");

        MigracionCloudinaryResultDTO result = await _listaApplication.MigrarListasACloudinaryAsync();
        return Ok(result);
    }

    [HttpGet]
    [Route("Lista-Torneo/{idTorneo}/{idUsuario}")]
    public async Task<ActionResult>GetListaTorneo(int idTorneo, int idUsuario)
    {
        if (!User.TryGetIdUsuario(out int idUsuarioSolicitante))
            return Unauthorized();

        // 400 y no 403: el interceptor del front manda a la página de error con cualquier 403
        if (!await _listaApplication.PuedeVerListaTorneoAsync(idTorneo, idUsuario, idUsuarioSolicitante))
            return BadRequest("Las listas de este torneo están ocultas.");

        ListaTorneoRequestDTO listaTorneoRequestDTO = new()
        {
            IdTorneo = idTorneo,
            IdUsuario = idUsuario
        };

        ListaDTO lista = await _listaApplication.GetListaTorneo(listaTorneoRequestDTO);
        // El front enseña este texto tal cual al pulsar en una lista que no existe
        if (lista is null)
        {
            return BadRequest("Todavía no ha entregado la lista.");
        }
        return Ok(lista);
    }
}
