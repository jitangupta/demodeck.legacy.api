using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Demodeck.Legacy.Api.Infrastructure;

namespace Demodeck.Legacy.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AppLogger.Info("Application starting", new
            {
                ServiceName = Environment.GetEnvironmentVariable("AppSettings__ServiceName") ?? "Demodeck.Legacy.Api",
                Environment = Environment.GetEnvironmentVariable("AppSettings__Environment") ?? "Development"
            });

            try
            {
                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(WebApiConfig.Register);
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);

                AppLogger.Info("Application started successfully");
            }
            catch (Exception ex)
            {
                AppLogger.Fatal(ex, "Application failed to start");
                throw;
            }
        }

        protected void Application_End()
        {
            AppLogger.Info("Application shutting down");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            if (exception != null)
            {
                AppLogger.Error(exception, "Unhandled exception occurred", new
                {
                    Url = HttpContext.Current?.Request?.Url?.ToString() ?? "Unknown",
                    Method = HttpContext.Current?.Request?.HttpMethod ?? "Unknown"
                });
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Generate correlation ID for request tracing
            var correlationId = HttpContext.Current.Request.Headers["X-Correlation-ID"]
                ?? HttpContext.Current.Request.Headers["X-Request-ID"]
                ?? Guid.NewGuid().ToString("N");

            HttpContext.Current.Items["CorrelationId"] = correlationId;

            // Add correlation ID to response headers
            HttpContext.Current.Response.Headers["X-Correlation-ID"] = correlationId;

            // Handle CORS preflight (OPTIONS) requests
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.StatusCode = 200;
                HttpContext.Current.Response.End();
            }
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            var path = HttpContext.Current?.Request?.Path;
            if (path != null && !path.Contains("/api/health"))
            {
                var statusCode = HttpContext.Current.Response.StatusCode;

                if (statusCode >= 500)
                {
                    AppLogger.Error("Request completed with server error", new
                    {
                        Method = HttpContext.Current.Request.HttpMethod,
                        Path = path,
                        StatusCode = statusCode
                    });
                }
                else if (statusCode >= 400)
                {
                    AppLogger.Warn("Request completed with client error", new
                    {
                        Method = HttpContext.Current.Request.HttpMethod,
                        Path = path,
                        StatusCode = statusCode
                    });
                }
            }
        }
    }
}
