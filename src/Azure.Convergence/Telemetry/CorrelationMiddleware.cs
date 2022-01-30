using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.ApplicationInsights
{
    public class AppInsightsCorrelationMiddleware : TelemetryCorrelationMiddleware
    {
        protected override void Process(HttpContext context, RouteEndpoint endpoint)
        {
            var telemetry = context.Features.Get<RequestTelemetry>()!;
            if (string.IsNullOrEmpty(telemetry.Name) && !string.IsNullOrEmpty(endpoint.RoutePattern.RawText))
            {
                telemetry.Name = context.Request.Method + " /" + endpoint.RoutePattern.RawText.TrimStart('/');
            }
        }

        protected override void ProcessNotFound(HttpContext context, string url)
        {
            var telemetry = context.Features.Get<RequestTelemetry>()!;
            if (string.IsNullOrEmpty(telemetry.Name))
            {
                telemetry.Name = context.Request.Method + " /" + (url ?? "").TrimStart('/');
            }
        }
    }
}
