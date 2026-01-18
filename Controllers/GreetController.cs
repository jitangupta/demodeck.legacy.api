using System;
using System.Web.Http;

namespace Demodeck.Legacy.Api.Controllers
{
    [RoutePrefix("api/greet")]
    public class GreetController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok(new
            {
                message = "Hello from legacy app",
                timestamp = DateTime.UtcNow.ToString("o"),
                source = "Demodeck.Legacy.Api (.NET Framework 4.8)"
            });
        }

        [HttpGet]
        [Route("{name}")]
        public IHttpActionResult Get(string name)
        {
            return Ok(new
            {
                message = $"Hello {name} from legacy app",
                timestamp = DateTime.UtcNow.ToString("o"),
                source = "Demodeck.Legacy.Api (.NET Framework 4.8)"
            });
        }
    }
}
