using System.Text.Json.Serialization;

namespace SatelliteSite.Models
{
    /// <summary>
    /// The model class for general response.
    /// </summary>
    public class GeneralResponse
    {
        /// <summary>
        /// Whether the operation is succeeded
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Gives the reason of failure
        /// </summary>
        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }
}
