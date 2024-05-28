using AS.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AS.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FaccionesController : ControllerBase
    {
        private readonly DbamonsulContext _dbamonsulContext;

        public FaccionesController(DbamonsulContext dbamonsulContext)
        {
            _dbamonsulContext = dbamonsulContext;
        }

        // GET: api/<FaccionesController>
        [HttpGet]
        [Route("Facciones")]
        public async Task<IActionResult> GetFacciones()
        {
            var facciones = await _dbamonsulContext.Faccions.ToListAsync();
            return Ok(facciones);
        }
    }
}
