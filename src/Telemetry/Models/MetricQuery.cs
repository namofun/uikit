using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SatelliteSite.TelemetryModule.Models
{
    public class MetricQuery
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("parameters")]
        public Dictionary<string, string> Parameters { get; set; }

        public MetricQuery(string id, string metricId,
            string aggregation = null, string segment = null,
            string timespan = "P1D", string interval = "PT30M")
        {
            Id = id;
            Parameters = Of(aggregation, segment, timespan, interval);
            Parameters.Add(nameof(metricId), metricId);
        }

        public static Dictionary<string, string> Of(
            string aggregation = null,
            string segment = null,
            string timespan = "P1D",
            string interval = "PT30M")
        {
            var result = new Dictionary<string, string>
            {
                { nameof(timespan), timespan },
                { nameof(interval), interval },
            };

            if (aggregation != null)
            {
                result.Add(nameof(aggregation), aggregation);
            }

            if (segment != null)
            {
                result.Add(nameof(segment), segment);
            }

            return result;
        }
    }
}
