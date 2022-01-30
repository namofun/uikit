using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationInsightsTelemetryExtensions
    {
        public static IServiceCollection AddApplicationInsights(this IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();

            services.EnsureSingleton<ITelemetryClient>();
            services.ReplaceSingleton<ITelemetryClient, ApplicationInsightsTelemetryClient>();

            services.EnsureSingleton<TelemetryCorrelationMiddleware>();
            services.ReplaceSingleton<TelemetryCorrelationMiddleware, AppInsightsCorrelationMiddleware>();

            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.EnableSqlCommandTextInstrumentation = true;
            });

            if (Environment.GetEnvironmentVariable("APPINSIGHTS_CLOUDROLE") is string cloudRole)
            {
                string? roleInstance = Environment.GetEnvironmentVariable("APPINSIGHTS_CLOUDROLEINSTANCE");
                services.AddSingleton<ITelemetryInitializer>(new CloudRoleInitializer(cloudRole, roleInstance));
            }

            return services;
        }
    }
}
