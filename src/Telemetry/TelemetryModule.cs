﻿using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.Services;
using SatelliteSite.TelemetryModule.Services;

namespace SatelliteSite.TelemetryModule
{
    public class TelemetryModule : AbstractModule
    {
        public override string Area => "Telemetry";

        public override void Initialize()
        {
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();

            services.EnsureSingleton<ITelemetryClient>();
            services.ReplaceSingleton<ITelemetryClient, ApplicationInsightsTelemetryClient>();

            services.EnsureSingleton<TelemetryCorrelationMiddleware>();
            services.ReplaceSingleton<TelemetryCorrelationMiddleware, AppInsightsCorrelationMiddleware>();

            services.AddHttpClient<TelemetryDataClient>();

            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.EnableSqlCommandTextInstrumentation = true;
            });
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            var options = endpoints.ServiceProvider.GetOptions<ApplicationInsightsDisplayOptions>();
            if (string.IsNullOrEmpty(options.Value.ApiKey)) return;

            endpoints.MapControllers();
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            var options = menus.ServiceProvider.GetOptions<ApplicationInsightsDisplayOptions>();
            if (string.IsNullOrEmpty(options.Value.ApiKey)) return;

            menus.Submenu(MenuNameDefaults.DashboardDocuments, menu =>
            {
                menu.HasEntry(110)
                    .HasTitle(string.Empty, "Application Insights")
                    .HasLink("Dashboard", "ApplicationInsights", "Index")
                    .RequireRoles("Administrator");
            });
        }
    }
}
