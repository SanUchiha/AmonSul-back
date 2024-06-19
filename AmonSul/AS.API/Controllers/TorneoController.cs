using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TorneoController : ControllerBase
    {
        // GET: api/<TorneoController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<TorneoController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<TorneoController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<TorneoController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<TorneoController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
