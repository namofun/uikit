using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SatelliteSite.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace SatelliteSite.TelemetryModule.Services
{
    public class TelemetryDataClient
    {
        public HttpClient HttpClient { get; }

        public ApplicationInsightsDisplayOptions Options { get; }

        public TelemetryDataClient(HttpClient client, IOptions<ApplicationInsightsDisplayOptions> options)
        {
            HttpClient = client;
            Options = options.Value;

            client.BaseAddress = new System.Uri("https://api.applicationinsights.io/v1/apps/" + Options.ApplicationId + "/");
            client.DefaultRequestHeaders.Add("X-Api-Key", Options.ApiKey);
        }

        public IActionResult PostRequest(string requestUri, QueryString query, HttpContent content)
        {
            requestUri += query.Value.Replace("&amp;", "&");
            return new HttpResponseMessageResult(HttpClient.PostAsync(requestUri, content));
        }

        public IActionResult GetRequest(string requestUri, QueryString query)
        {
            requestUri += query.Value?.Replace("&amp;", "&");
            return new HttpResponseMessageResult(HttpClient.GetAsync(requestUri));
        }

        private class HttpResponseMessageResult : IActionResult
        {
            private readonly Task<HttpResponseMessage> _responseTask;

            public HttpResponseMessageResult(Task<HttpResponseMessage> message)
                => _responseTask = message;

            public async Task ExecuteResultAsync(ActionContext context)
            {
                using var resp = await _responseTask;
                
                context.HttpContext.Response.StatusCode = (int)resp.StatusCode;
                context.HttpContext.Response.ContentLength = resp.Content.Headers.ContentLength;
                context.HttpContext.Response.ContentType = resp.Content.Headers.ContentType.MediaType;
                await resp.Content.CopyToAsync(context.HttpContext.Response.Body);
            }
        }
    }
}
