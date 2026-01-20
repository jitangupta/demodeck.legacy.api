using System.Web.Http.Filters;

namespace Demodeck.Legacy.Api.Infrastructure
{
    /// <summary>
    /// Global exception filter for Web API controllers.
    /// Logs all unhandled exceptions with full context.
    /// </summary>
    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            var request = context.Request;
            var correlationId = System.Web.HttpContext.Current?.Items["CorrelationId"]?.ToString() ?? "unknown";

            AppLogger.Error(context.Exception, "Unhandled Web API exception", new
            {
                Method = request.Method.Method,
                RequestUri = request.RequestUri?.ToString(),
                Controller = context.ActionContext.ControllerContext.ControllerDescriptor.ControllerName,
                Action = context.ActionContext.ActionDescriptor.ActionName,
                CorrelationId = correlationId
            });

            base.OnException(context);
        }
    }
}
