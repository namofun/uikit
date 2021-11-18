using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SatelliteSite.TelemetryModule.Services
{
    public class AppInsightsCorrelationMiddleware : TelemetryCorrelationMiddleware
    {
        protected override void Process(HttpContext context, RouteEndpoint endpoint)
        {
            var telemetry = context.Features.Get<RequestTelemetry>();
            if (string.IsNullOrEmpty(telemetry.Name))
            {
                telemetry.Name = context.Request.Method + " /" + endpoint.RoutePattern.RawText.TrimStart('/');
            }
        }

        protected override void ProcessNotFound(HttpContext context, string url)
        {
            var telemetry = context.Features.Get<RequestTelemetry>();
            if (string.IsNullOrEmpty(telemetry.Name))
            {
                telemetry.Name = context.Request.Method + " /" + (url ?? "").TrimStart('/');
            }
        }
    }
}
