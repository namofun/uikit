using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing
{
    /// <summary>
    /// The feature implementation providing url rewriting values.
    /// </summary>
    internal class UrlRewritingFeature : IUrlRewritingFeature
    {
        /// <inheritdoc />
        public PathString Path { get; }

        /// <inheritdoc />
        public QueryString Query { get; }

        /// <inheritdoc />
        public HostString Host { get; }

        /// <inheritdoc />
        public PathString PathBase { get; }

        /// <summary>
        /// Instantiate the feature with the original request.
        /// </summary>
        /// <param name="request">The original request.</param>
        public UrlRewritingFeature(HttpRequest request)
        {
            Host = request.Host;
            Query = request.QueryString;
            Path = request.Path;
            PathBase = request.PathBase;
        }
    }
}
