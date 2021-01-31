using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.TelemetryModule.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace SatelliteSite.TelemetryModule.Dashboards
{
    [Area("Dashboard")]
    [Authorize(Roles = "Administrator")]
    [Route("[area]/[controller]")]
    public class ApplicationInsightsController : ViewControllerBase
    {
        private readonly TelemetryDataClient _client;
        public ApplicationInsightsController(TelemetryDataClient client)
            => _client = client;


        [HttpGet]
        public IActionResult Index()
        {
            return Ok();
        }


        [HttpGet("[action]/{category}/{item}")]
        public IActionResult Metrics(string category, string item, string timespan = "PT12H", string interval = "PT30M", string aggregation = null)
        {
            var query = QueryString.Create(new[]
            {
                new KeyValuePair<string, string>(nameof(timespan), timespan),
                new KeyValuePair<string, string>(nameof(interval), interval),
            });

            if (aggregation != null)
            {
                query = query.Add(nameof(aggregation), aggregation);
            }

            return _client.GetRequest($"metrics/{category}/{item}", query);
        }


        [HttpGet("metrics/metadata")]
        public IActionResult MetricsMetadata()
        {
            return _client.GetRequest("metrics/metadata", QueryString.Empty);
        }


        [HttpGet("[action]")]
        public IActionResult Query(string kql, string timespan = "P1D")
        {
            var query = (kql ?? string.Empty).Replace("\n", "").Replace("\r", "");
            return _client.PostRequest(
                requestUri: "query",
                query: QueryString.Create(nameof(timespan), timespan),
                content: new StringContent(new { query }.ToJson(), Encoding.UTF8, "application/json"));
        }
    }
}
