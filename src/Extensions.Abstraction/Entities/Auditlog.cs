#nullable disable
using System;
using System.Text.Json.Serialization;

namespace SatelliteSite.Entities
{
    /// <summary>
    /// The auditlog for recording operations on websites.
    /// </summary>
    public class Auditlog
    {
        /// <summary>
        /// The ID of log.
        /// </summary>
        [JsonPropertyName("id")]
        public int LogId { get; set; }

        /// <summary>
        /// The time of event happened.
        /// </summary>
        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// The username who emitted this event.
        /// </summary>
        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        /// <summary>
        /// The contest id for events.
        /// </summary>
        [JsonPropertyName("contestId")]
        public int? ContestId { get; set; }

        /// <summary>
        /// The data type for this event source.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AuditlogType DataType { get; set; }

        /// <summary>
        /// The id of data target.
        /// </summary>
        [JsonPropertyName("target")]
        public string DataId { get; set; }

        /// <summary>
        /// The target action.
        /// </summary>
        [JsonPropertyName("action")]
        public string Action { get; set; }

        /// <summary>
        /// The extra comment on events.
        /// </summary>
        [JsonPropertyName("comment")]
        public string ExtraInfo { get; set; }
    }
}
