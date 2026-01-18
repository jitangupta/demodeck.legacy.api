using System;
using System.Configuration;
using System.Web.Http;

namespace Demodeck.Legacy.Api.Controllers
{
    [RoutePrefix("api/health")]
    public class HealthController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            // Get values from config, with environment variable override support
            var serviceName = GetConfigValue("ServiceName", "Demodeck.Legacy.Api");
            var version = GetConfigValue("Version", "1.0.0");
            var environment = GetConfigValue("Environment", "Production");

            return Ok(new
            {
                status = "Healthy",
                service = serviceName,
                version = version,
                environment = environment,
                timestamp = DateTime.UtcNow.ToString("o"),
                machineName = Environment.MachineName,
                osVersion = Environment.OSVersion.ToString(),
                platform = "Windows"
            });
        }

        private string GetConfigValue(string key, string defaultValue)
        {
            // First try environment variable (Kubernetes standard format with double underscore)
            var envKey = $"AppSettings__{key}";
            var envValue = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }

            // Also try single underscore format
            envKey = $"AppSettings_{key}";
            envValue = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }

            // Also try direct key name
            envValue = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }

            // Fall back to Web.config
            var configValue = ConfigurationManager.AppSettings[key];
            return !string.IsNullOrEmpty(configValue) ? configValue : defaultValue;
        }
    }
}
