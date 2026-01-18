using System.Web.Http;

namespace Demodeck.Legacy.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Enable simple CORS globally (avoids external Cors package)
            config.MessageHandlers.Add(new SimpleCorsHandler());

            // Enable attribute routing
            config.MapHttpAttributeRoutes();

            // Convention-based routing fallback
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Return JSON by default
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
