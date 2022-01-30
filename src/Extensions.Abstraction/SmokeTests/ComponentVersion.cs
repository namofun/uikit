using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.Diagnostics.SmokeTests
{
    /// <summary>
    /// The smoke test target of component version.
    /// </summary>
    public class ComponentVersion
    {
        /// <summary>
        /// The assembly name
        /// </summary>
        [JsonPropertyName("name")]
        public string? AssemblyName { get; set; }

        /// <summary>
        /// The assembly version
        /// </summary>
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        /// <summary>
        /// The git branch
        /// </summary>
        [JsonPropertyName("branch")]
        public string? Branch { get; set; }

        /// <summary>
        /// The git commit ID
        /// </summary>
        [JsonPropertyName("commitId")]
        public string? CommitId { get; set; }

        /// <summary>
        /// The signing public key
        /// </summary>
        [JsonPropertyName("publicKey")]
        public string? PublicKey { get; set; }
    }

    /// <summary>
    /// The system component list.
    /// </summary>
    public class SystemComponent
    {
        /// <summary>
        /// Whether current system enables Razor runtime compilation
        /// </summary>
        [JsonPropertyName("razorRuntimeCompilation")]
        public bool RazorRuntimeCompilation { get; set; }

        /// <summary>
        /// All the loaded component versions
        /// </summary>
        [JsonPropertyName("components")]
        public List<ComponentVersion> ComponentVersions { get; set; }

        /// <summary>
        /// Initialize the system component.
        /// </summary>
#pragma warning disable CS8618
        public SystemComponent()
#pragma warning restore CS8618
        {
        }
    }
}
