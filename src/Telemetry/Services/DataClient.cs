using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace SatelliteSite.TelemetryModule.Services
{
    public class TelemetryDataClient
    {
        public HttpClient HttpClient { get; }

        public UrlEncoder UrlEncoder { get; }

        public ApplicationInsightsDisplayOptions Options { get; }

        public TelemetryDataClient(
            HttpClient client,
            IOptions<ApplicationInsightsDisplayOptions> options,
            UrlEncoder urlEncoder)
        {
            HttpClient = client;
            Options = options.Value;
            UrlEncoder = urlEncoder;

            client.BaseAddress = new Uri("https://api.applicationinsights.io/v1/apps/" + Options.ApplicationId + "/");
            client.DefaultRequestHeaders.Add("X-Api-Key", Options.ApiKey);
        }

        public IActionResult GetRequest(string requestUri, Dictionary<string, string> param = null)
        {
            if (param != null && param.Count > 0)
                requestUri += "?" + string.Join('&', param.Select(a => a.Key + "=" + UrlEncoder.Encode(a.Value)));
            return SendRequest(new HttpRequestMessage(HttpMethod.Get, requestUri));
        }

        public IActionResult PostRequest<T>(string requestUri, T value)
        {
            var content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(value));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return SendRequest(new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content });
        }

        public IActionResult SendRequest(HttpRequestMessage request)
        {
            return new HttpResponseMessageResult(HttpClient, request);
        }

        private class HttpResponseMessageResult : IActionResult
        {
            private readonly HttpClient _httpClient;
            private readonly HttpRequestMessage _request;

            public HttpResponseMessageResult(HttpClient httpClient, HttpRequestMessage request)
                => (_httpClient, _request) = (httpClient, request);

            public async Task ExecuteResultAsync(ActionContext context)
            {
                using var resp = await _httpClient.SendAsync(_request, context.HttpContext.RequestAborted);
                
                context.HttpContext.Response.StatusCode = (int)resp.StatusCode;
                context.HttpContext.Response.ContentLength = resp.Content.Headers.ContentLength;
                context.HttpContext.Response.ContentType = resp.Content.Headers.ContentType.MediaType;
                await resp.Content.CopyToAsync(context.HttpContext.Response.Body);
            }
        }
    }
}
