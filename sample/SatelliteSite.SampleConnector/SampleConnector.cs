using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.SampleConnector.Services;
using System;

[assembly: AffiliateTo(
    typeof(SatelliteSite.SampleConnector.SampleConnector),
    typeof(SatelliteSite.SampleModule.SampleModule))]

namespace SatelliteSite.SampleConnector
{
    public class SampleConnector : AbstractConnector
    {
        public override string Area => "Sample";

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Submenu(MenuNameDefaults.DashboardUsers, menu =>
            {
                menu.HasEntry(-1000)
                    .HasLink("Sample", "Test", "Index")
                    .HasTitle(string.Empty, "Sample Connector");
            });
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddHttpClient<AzureManagementClient>()
                .AddAzureAuthHandler(new[] { "https://management.azure.com/.default" })
                .ConfigureHttpClient(httpClient => httpClient.BaseAddress = new Uri("https://management.azure.com/"));

            services.AddOptions<DefaultAzureCredentialOptions>()
                .Configure(options => options.VisualStudioTenantId = "65f7f058-fc47-4803-b7f6-1dd03a071b50");

            services.AddHttpClient<LogicAppsClient>()
                .AddAzureAuthHandler(new[] { "https://logic.azure.com/.default" });
        }
    }
}
