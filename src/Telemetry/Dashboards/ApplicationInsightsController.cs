using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.TelemetryModule.Model;
using SatelliteSite.TelemetryModule.Services;

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
            return View();
        }


        [HttpGet("metrics/{category}/{item}")]
        public IActionResult Metrics(string category, string item,
            string timespan = "P1D", string interval = "PT30M",
            string aggregation = null, string segment = null)
        {
            return _client.GetRequest($"metrics/{category}/{item}",
                MetricQuery.Of(aggregation, segment, timespan, interval));
        }


        [HttpGet("metrics/metadata")]
        public IActionResult MetricsMetadata()
        {
            return _client.GetRequest("metrics/metadata");
        }


        [HttpGet("custom/general")]
        public IActionResult CustomMetric1()
        {
            return _client.PostRequest("metrics", new[]
            {
                new MetricQuery("Failed requests", "requests/failed"),
                new MetricQuery("Server response time", "requests/duration"),
                new MetricQuery("Server exceptions", "exceptions/server"),
                new MetricQuery("Dependency failures", "dependencies/failed"),
                new MetricQuery("Process CPU utilization", "performanceCounters/processCpuPercentage", "avg,max,min"),
            });
        }


        [HttpGet("query")]
        public IActionResult Query(string kql, string timespan = "P1D")
        {
            var query = (kql ?? string.Empty).Replace("\n", "").Replace("\r", "");
            return _client.PostRequest("query", new { query, timespan });
        }
    }
}
