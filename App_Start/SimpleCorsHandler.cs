using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Demodeck.Legacy.Api
{
    // Simple CORS handler to allow cross-origin requests without external package
    public class SimpleCorsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // If it's an OPTIONS request, return OK with the necessary headers
            if (request.Method == HttpMethod.Options)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                AddCorsHeaders(response);
                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(response);
                return tcs.Task;
            }

            return base.SendAsync(request, cancellationToken).ContinueWith(t =>
            {
                var resp = t.Result;
                AddCorsHeaders(resp);
                return resp;
            }, cancellationToken);
        }

        private static void AddCorsHeaders(HttpResponseMessage response)
        {
            if (!response.Headers.Contains("Access-Control-Allow-Origin"))
                response.Headers.Add("Access-Control-Allow-Origin", "*");

            if (!response.Headers.Contains("Access-Control-Allow-Methods"))
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            if (!response.Headers.Contains("Access-Control-Allow-Headers"))
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept, Authorization");
        }
    }
}
