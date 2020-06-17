using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace SatelliteSite.SampleModule
{
    public class SampleModule : AbstractModule
    {
        public override void RegisterEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.Map("/", async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }

        public override void RegisterServices(IServiceCollection services)
        {

        }
    }
}
