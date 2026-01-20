using System.Web.Http;
using Demodeck.Legacy.Api.Infrastructure;

namespace Demodeck.Legacy.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Register global exception filter for structured logging
            config.Filters.Add(new GlobalExceptionFilter());

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
