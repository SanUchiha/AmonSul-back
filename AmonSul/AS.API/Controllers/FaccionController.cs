using AS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FaccionController : ControllerBase
    {
        private readonly IFaccionApplication _faccionApplication;

        public FaccionController(IFaccionApplication faccionApplication)
        {
            _faccionApplication = faccionApplication;
        }

        /// <summary>
        /// Obtener las facciones
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("")]
        public async Task<IActionResult> GetFacciones()
        {
            var response = await _faccionApplication.GetFacciones();
            return Ok(response);
        }
    }
}
