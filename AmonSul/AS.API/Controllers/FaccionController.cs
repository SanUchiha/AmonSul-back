using AS.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AS.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FaccionController : ControllerBase
    {
        private readonly DbamonsulContext _dbamonsulContext;

        public FaccionController(DbamonsulContext dbamonsulContext)
        {
            _dbamonsulContext = dbamonsulContext;
        }

        /// <summary>
        /// Obtener las facciones
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Facciones")]
        public async Task<IActionResult> GetFacciones()
        {
            var facciones = await _dbamonsulContext.Faccions.ToListAsync();
            return Ok(facciones);
        }
    }
}
