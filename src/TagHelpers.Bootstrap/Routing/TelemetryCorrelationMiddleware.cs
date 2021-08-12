using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
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
            if (context.Features.Get<IStatusCodeReExecuteFeature>() == null)
            {
                var endpoint = context.GetEndpoint();

                if (endpoint == null)
                {
                    ProcessNotFound(context, context.Request.Path.Value);
                }
                else if (endpoint is RouteEndpoint routeEndpoint)
                {
                    switch (routeEndpoint.Metadata.GetMetadata<TrackAvailabilityMetadata>()?.Track ?? default)
                    {
                        case TrackAvailability.Default:
                        case TrackAvailability.Fallback:
                            Process(context, routeEndpoint);
                            break;

                        case TrackAvailability.ErrorHandler:
                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    // It's another type of endpoint but not route endpoint. So strange.
                }
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

        /// <summary>
        /// Process the not found requests with the context.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="url">The request url.</param>
        protected virtual void ProcessNotFound(HttpContext context, string url)
        {
        }
    }
}
