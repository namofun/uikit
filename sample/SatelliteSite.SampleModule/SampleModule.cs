using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

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

            endpoints.WithErrorHandler("Sample", "Main")
                .MapFallbackNotFound("/sample/{**slug}")
                .MapStatusCode("/sample/{**slug}");

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

        }
    }
}
