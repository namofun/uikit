using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.Services;

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
            services.EnsureSingleton<ITelemetryClient>();
            services.ReplaceSingleton<ITelemetryClient, ApplicationInsightsTelemetryClient>();
            services.AddApplicationInsightsTelemetry();
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            var options = endpoints.ServiceProvider.GetOptions<ApplicationInsightsDisplayOptions>();
            if (!string.IsNullOrEmpty(options.Value.ApiKey))
            {
                endpoints.MapControllers();
            }
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
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
