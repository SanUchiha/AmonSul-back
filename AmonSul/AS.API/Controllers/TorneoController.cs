using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers
{
    [Route("[controller]")]
    //[Authorize]
    [ApiController]
    public class TorneoController : ControllerBase
    {
        private readonly ITorneoApplication _torneoApplication;

        public TorneoController(ITorneoApplication torneoApplication)
        {
            _torneoApplication = torneoApplication;
        }

        /// <summary>
        /// Obtener los torneos
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetTorneos()
        {
            var response = await _torneoApplication.GetTorneos();
            
            if(response == null) return NotFound();

            return Ok(response);
        }
    }
}
