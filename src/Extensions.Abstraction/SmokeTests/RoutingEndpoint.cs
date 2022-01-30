using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.Diagnostics.SmokeTests
{
    /// <summary>
    /// The smoke test target of routing endpoints.
    /// </summary>
    public class RoutingEndpoint
    {
        /// <summary>
        /// The MVC area of endpoint
        /// </summary>
        [JsonPropertyName("area")]
        public string? Area { get; set; }

        /// <summary>
        /// Whether this is MVC endpoint
        /// </summary>
        [JsonPropertyName("nonMvc")]
        public bool NonMvc { get; set; }

        /// <summary>
        /// Whether this endpoint is used in matching
        /// </summary>
        [JsonPropertyName("inert")]
        public bool Inert { get; set; }

        /// <summary>
        /// The alternative endpoint name
        /// </summary>
        [JsonPropertyName("formalName")]
        public string? AlternativeName { get; set; }

        /// <summary>
        /// The order of endpoint
        /// </summary>
        [JsonPropertyName("order")]
        public int Order { get; set; }

        /// <summary>
        /// The allowed methods for HTTP requests
        /// </summary>
        [JsonPropertyName("allowedMethods")]
        public string[] AllowedMethods { get; set; } = Array.Empty<string>();

        /// <summary>
        /// The routing pattern
        /// </summary>
        [JsonPropertyName("routePattern")]
        public string? RoutePattern { get; set; }

        /// <summary>
        /// The endpoint description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Whether this endpoint is used for routing
        /// </summary>
        [JsonPropertyName("routing")]
        public bool UsedForRouting { get; set; }
    }

    /// <summary>
    /// The group of routing endpoints.
    /// </summary>
    public class RoutingGroup
    {
        /// <summary>
        /// The routing group name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// The endpoint list
        /// </summary>
        [JsonPropertyName("items")]
        public List<RoutingEndpoint> Endpoints { get; set; }

        /// <summary>
        /// Initialize the routing group.
        /// </summary>
#pragma warning disable CS8618
        public RoutingGroup()
#pragma warning restore CS8618
        {
        }
    }
}
