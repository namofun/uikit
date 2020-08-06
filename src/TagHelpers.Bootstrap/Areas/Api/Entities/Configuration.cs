#nullable disable

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
        public string Name { get; set; }

        /// <summary>
        /// The value of configuration (in JSON string)
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The type of value
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The display priority
        /// </summary>
        public int DisplayPriority { get; set; }

        /// <summary>
        /// Whether to show on the configuration page
        /// </summary>
        public bool Public { get; set; }

        /// <summary>
        /// The category of configuration
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The description of configuration item
        /// </summary>
        public string Description { get; set; }
    }
}
