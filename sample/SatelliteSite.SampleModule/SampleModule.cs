using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite;
using SatelliteSite.SampleModule.Components.Weather;
using SatelliteSite.SampleModule.Services;
using System.Threading.Tasks;

[assembly: ConfigurationInteger(1, "Weather", "random_count", 5, "The count of random weather items.")]
[assembly: ConfigurationString(2, "Weather", "default_name", "Hello", "The name of default.")]

namespace SatelliteSite.SampleModule
{
    public class SampleModule : AbstractModule
    {
        public override string Area => "Sample";

        public override void Initialize()
        {
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();

            endpoints.MapApiDocument(
                name: "sample",
                title: "Sample Module",
                description: "The API for sample module",
                version: "1.0");

            endpoints.WithErrorHandler("Sample", "Main")
                .MapFallbackNotFound("/sample/{**slug}")
                .MapStatusCode("/sample/{**slug}")
                .MapFallbackNotFound("/route-test/{id:int}")
                .MapFallbackNotFound("/route-test/{id:alpha}");

            endpoints.MapRequestDelegate("/weather/checker", async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });

            endpoints.MapRequestDelegate("/sample/world", context =>
            {
                context.Response.StatusCode = 402;
                return Task.CompletedTask;
            });
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<ForecastService>();
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Menu(MenuNameDefaults.DashboardNavbar, menu =>
            {
                menu.HasEntry(0)
                    .HasTitle("fas fa-server", "Sample")
                    .HasLink("Dashboard", "Weather", "Change")
                    .ActiveWhenController("Weather")
                    .HasIdentifier("menu_weather");
            });

            menus.Submenu(MenuNameDefaults.DashboardConfigurations, menu =>
            {
                menu.HasEntry(0)
                    .HasTitle("fas fa-server", "Sample")
                    .HasLink("Dashboard", "Weather", "Change")
                    .ActiveWhenController("Weather")
                    .HasIdentifier("menu_weather");
            });

            menus.Component("Component_UserDetail")
                .HasComponent<WeatherViewComponent>(10);

            menus.Component("Component_DashboardUserDetail")
                .HasComponent<WeatherViewComponent>(10);
        }
    }
}
