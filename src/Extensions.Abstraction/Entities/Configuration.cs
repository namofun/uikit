#nullable disable
using System.Text.Json.Serialization;

namespace SatelliteSite.Entities
{
    /// <summary>
    /// The entity for configurations.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// The name of configuration
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// The value of configuration (in JSON string)
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; }

        /// <summary>
        /// The type of value
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// The display priority
        /// </summary>
        [JsonPropertyName("displayPriority")]
        public int DisplayPriority { get; set; }

        /// <summary>
        /// Whether to show on the configuration page
        /// </summary>
        [JsonPropertyName("isPublic")]
        public bool Public { get; set; }

        /// <summary>
        /// The category of configuration
        /// </summary>
        [JsonPropertyName("category")]
        public string Category { get; set; }

        /// <summary>
        /// The description of configuration item
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
