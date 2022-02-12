using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Hosting;
using System;

namespace Microsoft.AspNetCore.Hosting
{
    public static class ApplicationInsightsTelemetryExtensions
    {
        /// <summary>
        /// Adds Azure Application Insights as telemetry and correlation source.
        /// </summary>
        /// <param name="services">The service collection to enable application insights.</param>
        /// <returns>The service collection to chain configure calls.</returns>
        /// <remarks>
        /// Setting up several environment variables would help debugging.
        /// <list type="bullet"><c>APPINSIGHTS_CLOUDROLE</c> for setting cloud role.</list>
        /// <list type="bullet"><c>APPINSIGHTS_CLOUDROLEINSTANCE</c> for setting cloud role instance.</list>
        /// </remarks>
        public static IServiceCollection AddApplicationInsights(this IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();

            services.RemoveAll<ITelemetryClient>();
            services.TryAddSingleton<ITelemetryClient, ApplicationInsightsTelemetryClient>();

            services.RemoveAll<TelemetryCorrelationMiddleware>();
            services.TryAddSingleton<TelemetryCorrelationMiddleware, AppInsightsCorrelationMiddleware>();

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

        /// <summary>
        /// Adds Azure Application Insights as telemetry and correlation source.
        /// </summary>
        /// <param name="hostBuilder">The builder of host to enable application insights.</param>
        /// <returns>The host builder to chain configure calls.</returns>
        /// <remarks>
        /// Setting up several environment variables would help debugging.
        /// <list type="bullet"><c>APPINSIGHTS_CLOUDROLE</c> for setting cloud role.</list>
        /// <list type="bullet"><c>APPINSIGHTS_CLOUDROLEINSTANCE</c> for setting cloud role instance.</list>
        /// </remarks>
        public static IHostBuilder AddApplicationInsights(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(services => services.AddApplicationInsights());
        }
    }
}
