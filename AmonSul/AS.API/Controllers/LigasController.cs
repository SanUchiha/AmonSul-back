using AS.Application.Interfaces;
using AS.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LigasController(ILigaApplication ligaApplication) : ControllerBase
    {
        private readonly ILigaApplication _ligaApplication = ligaApplication;

        /// <summary>
        /// Obtener todas las ligas
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<Liga>>Get()
        {
            return await _ligaApplication.GetAllLigasAsync();
        }

        /// <summary>
        /// Obtener una liga por Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{idLiga}")]
        public async Task<Liga?> Get(int idLiga)
        {
            return await _ligaApplication.GetLigaByIdAsync(idLiga);
        }
        /// <summary>
        /// Obtener los torneos de una liga
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        // POST api/<LigasController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<LigasController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<LigasController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
