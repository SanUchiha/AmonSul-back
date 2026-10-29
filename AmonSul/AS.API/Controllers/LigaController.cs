using AS.API.Filters;
using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers;

/// <summary>
/// Gestión de los torneos con formato Liga. Todo lo hace solo el organizador; si no se puede, 400 con el motivo
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LigaController(ILigaApplication ligaApplication) : ControllerBase
{
    private readonly ILigaApplication _ligaApplication = ligaApplication;

    [HttpPost("{idTorneo}/Generar")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> Generar(int idTorneo) =>
        Resultado(await _ligaApplication.GenerarAsync(idTorneo));

    [HttpDelete("{idTorneo}/Jornadas")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> EliminarJornadas(int idTorneo) =>
        Resultado(await _ligaApplication.EliminarJornadasAsync(idTorneo));

    [HttpPost("{idTorneo}/Retirar/{idInscripcion}")]
    [ServiceFilter(typeof(AdminTorneoFilter))]
    public async Task<IActionResult> RetirarJugador(int idTorneo, int idInscripcion) =>
        Resultado(await _ligaApplication.RetirarJugadorAsync(idTorneo, idInscripcion));

    private IActionResult Resultado((bool Ok, string? Error) resultado) =>
        resultado.Ok ? Ok() : BadRequest(resultado.Error);
}
