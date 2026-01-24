using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Demodeck.Legacy.Api.Infrastructure;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Demodeck.Legacy.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static TracerProvider _tracerProvider;

        protected void Application_Start()
        {
            AppLogger.Info("Application starting", new
            {
                ServiceName = Environment.GetEnvironmentVariable("AppSettings__ServiceName") ?? "Demodeck.Legacy.Api",
                Environment = Environment.GetEnvironmentVariable("AppSettings__Environment") ?? "Development"
            });

            try
            {
                // Initialize OpenTelemetry Tracing
                InitializeOpenTelemetry();

                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(WebApiConfig.Register);
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);

                AppLogger.Info("Application started successfully with OpenTelemetry tracing");
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
            _tracerProvider?.Dispose();
        }

        private void InitializeOpenTelemetry()
        {
            var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME")
                ?? Environment.GetEnvironmentVariable("AppSettings__ServiceName")
                ?? "legacy-api";
            var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
                ?? "http://tempo.observability.svc.cluster.local:4317";
            var environment = Environment.GetEnvironmentVariable("AppSettings__Environment")
                ?? "Development";

            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName, serviceVersion: "1.0.0")
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment"] = environment,
                        ["service.namespace"] = "demodeck"
                    }))
                .AddAspNetInstrumentation(opts =>
                {
                    opts.RecordException = true;
                    opts.Filter = ctx => !ctx.Request.Path.Contains("/api/health");
                    // New API in v1.15.0: EnrichWithHttpRequest replaces Enrich
                    opts.EnrichWithHttpRequest = (activity, request) =>
                    {
                        var correlationId = HttpContext.Current?.Items["CorrelationId"]?.ToString();
                        if (!string.IsNullOrEmpty(correlationId))
                            activity?.SetTag("correlation.id", correlationId);
                    };
                    opts.EnrichWithHttpResponse = (activity, response) =>
                    {
                        activity?.SetTag("http.response.status_code", response.StatusCode);
                    };
                    opts.EnrichWithException = (activity, exception) =>
                    {
                        activity?.SetTag("error.type", exception.GetType().Name);
                        activity?.SetTag("error.message", exception.Message);
                    };
                })
                .AddHttpClientInstrumentation(opts => opts.RecordException = true)
                .AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint))
                .Build();

            AppLogger.Info("OpenTelemetry initialized", new { Endpoint = otlpEndpoint, ServiceName = serviceName });
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            if (exception != null)
            {
                // Record exception in the current trace span
                Activity.Current?.RecordException(exception);
                Activity.Current?.SetStatus(ActivityStatusCode.Error, exception.Message);

                AppLogger.Error(exception, "Unhandled exception occurred", new
                {
                    Url = HttpContext.Current?.Request?.Url?.ToString() ?? "Unknown",
                    Method = HttpContext.Current?.Request?.HttpMethod ?? "Unknown",
                    TraceId = Activity.Current?.TraceId.ToString()
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

            // Link correlation ID to trace context
            Activity.Current?.SetTag("correlation.id", correlationId);

            // Add trace ID to response headers for debugging
            if (Activity.Current != null)
            {
                HttpContext.Current.Response.Headers["X-Trace-ID"] = Activity.Current.TraceId.ToString();
            }

            // Set CORS headers for all requests
            var origin = HttpContext.Current.Request.Headers["Origin"];
            if (!string.IsNullOrEmpty(origin))
            {
                HttpContext.Current.Response.Headers["Access-Control-Allow-Origin"] = origin;
                HttpContext.Current.Response.Headers["Access-Control-Allow-Credentials"] = "true";
            }

            // Handle CORS preflight (OPTIONS) requests
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, PATCH, OPTIONS";
                HttpContext.Current.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With, X-Correlation-ID, X-Request-ID, X-Tenant, X-Tenant-Id, X-Tenant-Version";
                HttpContext.Current.Response.Headers["Access-Control-Max-Age"] = "86400";
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
