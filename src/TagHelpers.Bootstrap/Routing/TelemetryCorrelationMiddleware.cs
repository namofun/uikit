using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Routing
{
    /// <summary>
    /// Defines middleware that fix the telemetry request for the application's request pipeline.
    /// </summary>
    public class TelemetryCorrelationMiddleware : IMiddleware
    {
        /// <inheritdoc />
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Features.Get<IStatusCodeReExecuteFeature>() == null
                && context.GetEndpoint() is RouteEndpoint endpoint
                && (endpoint.Metadata.GetMetadata<TrackAvailabilityMetadata>()?.Track ?? default) == TrackAvailability.Default)
            {
                Process(context, endpoint);
            }

            return next(context);
        }

        /// <summary>
        /// Process the endpoint with the context.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="endpoint">The <see cref="RouteEndpoint"/>.</param>
        protected virtual void Process(HttpContext context, RouteEndpoint endpoint)
        {
        }
    }
}
