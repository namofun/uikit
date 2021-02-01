using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.TelemetryModule.Models;
using SatelliteSite.TelemetryModule.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        [ResponseCache(Duration = 300)]
        public async Task<ActionResult<MetricResponse>> CustomMetric1()
        {
            const string timespan = "PT30M";
            var req = _client.PostRequest("metrics", new[]
            {
                new MetricQuery("Requests", "requests/count", interval: timespan),
                new MetricQuery("Failed requests", "requests/failed", interval: timespan),
                new MetricQuery("Server response time", "requests/duration", interval: timespan),
                new MetricQuery("Server exceptions", "exceptions/server", interval: timespan),
                new MetricQuery("Dependency failures", "dependencies/failed", interval: timespan),
                new MetricQuery("Process CPU utilization", "performanceCounters/processCpuPercentage", "avg,max,min", interval: timespan),
                new MetricQuery("Average availability", "availabilityResults/availabilityPercentage", interval: timespan),
                new MetricQuery("Unique sessions", "sessions/count", interval: timespan),
                new MetricQuery("Unique users", "users/count", interval: timespan),
            });

            var res = await req.RequestAsync<List<MetricResponseWrapper>>(HttpContext.RequestAborted);

            var segments = new Dictionary<string, AnalyticSegment>();
            long start = res[0].Body.Value.Start.ToUnixTimeSeconds();
            long end = res[0].Body.Value.End.ToUnixTimeSeconds();
            for (int i = 0; i < res.Count; i++)
            {
                if (res[i].Status != 200)
                {
                    _client.LogRequestFailed(res[i].Id, res[i].Status);
                    continue;
                }

                var body = res[i].Body.Value;
                if (start != body.Start.ToUnixTimeSeconds() || end != body.End.ToUnixTimeSeconds() || timespan != body.Interval)
                {
                    _client.LogRequestFailed(res[i].Id, res[i].Status, "Field start/end/interval not match.");
                    continue;
                }

                for (int j = 0; j < body.Segments.Count; j++)
                {
                    var seg = body.Segments[j];
                    var segName = seg.Start.ToUnixTimeSeconds() + " - " + seg.End.ToUnixTimeSeconds();
                    if (!segments.TryGetValue(segName, out var segMain))
                    {
                        segMain = new AnalyticSegment(seg.Start, seg.End);
                        segments.Add(segName, segMain);
                    }

                    segMain.Combine(seg);
                }
            }

            var newMetric = new MetricResponse
            {
                Start = res[0].Body.Value.Start,
                End = res[^1].Body.Value.End,
                Interval = timespan,
                Segments = segments.Values.ToList()
            };

            newMetric.CommonTick = CreateTicks(newMetric.Start, newMetric.End);
            newMetric.Segments.Sort((a, b) => a.Tick.Value.CompareTo(b.Tick.Value));
            newMetric.Segments.ForEach(a => a.FillUp());
            return newMetric;

            static long[] CreateTicks(DateTimeOffset start, DateTimeOffset end)
            {
                Span<long> oks = stackalloc long[10];
                var b = start - start.TimeOfDay;
                int j = 0;
                for (int i = 0; i < 10; i++)
                {
                    if (start <= b && b <= end) oks[j++] = b.ToUnixTimeMilliseconds();
                    b += TimeSpan.FromHours(6);
                }

                return oks[0..j].ToArray();
            }
        }


        [HttpGet("query")]
        public IActionResult Query(string kql, string timespan = "P1D")
        {
            var query = (kql ?? string.Empty).Replace("\n", "").Replace("\r", "");
            return _client.PostRequest("query", new { query, timespan });
        }
    }
}
