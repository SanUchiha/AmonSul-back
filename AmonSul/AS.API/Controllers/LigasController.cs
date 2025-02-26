using AS.Application.DTOs;
using AS.Application.Interfaces;
using AS.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize]
    public class LigasController(ILigaApplication ligaApplication) : ControllerBase
    {
        private readonly ILigaApplication _ligaApplication = ligaApplication;

        [HttpGet]
        public async Task<List<Liga>>Get()
        {
            return await _ligaApplication.GetAllLigasAsync();
        }

        [HttpGet("{idLiga}")]
        public async Task<Liga?> Get(int idLiga)
        {
            return await _ligaApplication.GetLigaByIdAsync(idLiga);
        }

        [HttpGet("{id}/torneos")]
        public async Task<IActionResult> GetTorneosByLiga(int id)
        {
            List<LigaTorneo>? torneos = 
                await _ligaApplication.GetTorneosByIdLigaAsync(id);
            
            if (torneos is null) 
                return NotFound("No se encontraron torneos asociados a esta liga.");

            var result = torneos.Select(lt => new
            {
                lt.IdLiga,
                LigaNombre = lt.Liga?.NombreLiga,
                lt.IdTorneo,
                TorneoNombre = lt.Torneo?.NombreTorneo
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddTorneoToLiga([FromBody] LigaTorneoDTO ligaTorneoDTO) =>
            Ok(await _ligaApplication.AddTorneoToLigaAsync(ligaTorneoDTO));
    }
}
