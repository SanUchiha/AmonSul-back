using AS.API.Filters;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers;

/// <summary>
/// Gestión de los torneos con formato Copa. Todo lo hace solo el organizador; si no se puede, 400 con el motivo
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CopaController(ICopaApplication copaApplication) : ControllerBase
{
    private readonly ICopaApplication _copaApplication = copaApplication;

    [HttpPost("{idTorneo}/Grupos/Repartir")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> RepartirGrupos(int idTorneo) =>
        Resultado(await _copaApplication.RepartirGruposAsync(idTorneo));

    [HttpPut("{idTorneo}/Grupos/{idInscripcion}/{grupo}")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> CambiarGrupo(int idTorneo, int idInscripcion, int grupo) =>
        Resultado(await _copaApplication.CambiarGrupoAsync(idTorneo, idInscripcion, grupo));

    [HttpPost("{idTorneo}/Grupos/Generar")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> GenerarGrupos(int idTorneo) =>
        Resultado(await _copaApplication.GenerarGruposAsync(idTorneo));

    [HttpDelete("{idTorneo}/Jornadas")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> EliminarJornadas(int idTorneo) =>
        Resultado(await _copaApplication.EliminarJornadasAsync(idTorneo));

    [HttpPost("{idTorneo}/Retirar/{idInscripcion}")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> RetirarJugador(int idTorneo, int idInscripcion) =>
        Resultado(await _copaApplication.RetirarJugadorAsync(idTorneo, idInscripcion));

    [HttpPost("{idTorneo}/Eliminatoria/Generar")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> GenerarEliminatoria(int idTorneo) =>
        Resultado(await _copaApplication.GenerarEliminatoriaAsync(idTorneo));

    [HttpDelete("{idTorneo}/Eliminatoria")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> EliminarEliminatoria(int idTorneo) =>
        Resultado(await _copaApplication.EliminarEliminatoriaAsync(idTorneo));

    [HttpPost("{idTorneo}/Eliminatoria/{idPartida}/Pasa/{idUsuario}")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> ElegirQuienPasa(int idTorneo, int idPartida, int idUsuario) =>
        Resultado(await _copaApplication.ElegirQuienPasaAsync(idTorneo, idPartida, idUsuario));

    [HttpPost("{idTorneo}/Cerrar")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> Cerrar(int idTorneo) =>
        Resultado(await _copaApplication.CerrarAsync(idTorneo));

    private IActionResult Resultado((bool Ok, string? Error) resultado) =>
        resultado.Ok ? Ok() : BadRequest(resultado.Error);
}
