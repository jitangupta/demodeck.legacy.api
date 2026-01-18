using System;
using System.Configuration;
using System.Web.Http;

namespace Demodeck.Legacy.Api.Controllers
{
    /// <summary>
    /// Health check endpoint for Kubernetes liveness and readiness probes
    /// </summary>
    public class HealthController : ApiController
    {
        // GET api/health
        public IHttpActionResult Get()
        {
            // Read from environment variables first, fallback to AppSettings
            var serviceName = Environment.GetEnvironmentVariable("AppSettings__ServiceName")
                ?? ConfigurationManager.AppSettings["ServiceName"]
                ?? "Demodeck.Legacy.Api";

            var version = Environment.GetEnvironmentVariable("AppSettings__Version")
                ?? ConfigurationManager.AppSettings["Version"]
                ?? "1.0.0";

            var environment = Environment.GetEnvironmentVariable("AppSettings__Environment")
                ?? ConfigurationManager.AppSettings["Environment"]
                ?? "Development";

            return Ok(new
            {
                status = "Healthy",
                service = serviceName,
                version = version,
                environment = environment,
                timestamp = DateTime.UtcNow,
                machineName = Environment.MachineName
            });
        }
    }
}
