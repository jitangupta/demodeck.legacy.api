using System;
using System.Configuration;
using System.Web.Http;

namespace Demodeck.Legacy.Api.Controllers
{
    /// <summary>
    /// Greeting endpoint for demo purposes
    /// </summary>
    public class GreetController : ApiController
    {
        // GET api/greet
        public IHttpActionResult Get()
        {
            return Ok(new
            {
                message = "Hello from legacy app",
                timestamp = DateTime.UtcNow,
                source = "Demodeck.Legacy.Api (.NET Framework 4.8)"
            });
        }

        // GET api/greet/{name}
        public IHttpActionResult Get(string id)
        {
            return Ok(new
            {
                message = $"Hello {id}, from legacy app",
                timestamp = DateTime.UtcNow,
                source = "Demodeck.Legacy.Api (.NET Framework 4.8)"
            });
        }
    }
}
