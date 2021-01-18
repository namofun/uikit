#nullable enable
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing
{
    /// <summary>
    /// The feature providing url rewriting values.
    /// </summary>
    public interface IUrlRewritingFeature
    {
        /// <summary>
        /// The original request URL
        /// </summary>
        PathString Path { get; }

        /// <summary>
        /// The original path base
        /// </summary>
        PathString PathBase { get; }

        /// <summary>
        /// The original request URL
        /// </summary>
        QueryString Query { get; }

        /// <summary>
        /// The original request host
        /// </summary>
        HostString Host { get; }
    }
}
