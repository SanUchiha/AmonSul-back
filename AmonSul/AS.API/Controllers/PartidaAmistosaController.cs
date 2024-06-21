using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class PartidaAmistosaController : ControllerBase
    {
        private readonly IPartidaAmistosaApplication _partidaAmistosaApplication;

        public PartidaAmistosaController(IPartidaAmistosaApplication partidaAmistosaApplication)
        {
            _partidaAmistosaApplication = partidaAmistosaApplication;
        }

        /// <summary>
        /// Obtener las partidas amistosas
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetPartidasAmistosas()
        {
            var response = await _partidaAmistosaApplication.GetPartidasAmistosas();
            
            if(response == null) return NotFound();

            return Ok(response);
        }
    }
}
