using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.SampleModule.Models;
using SatelliteSite.Services;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SatelliteSite.Tests
{
    public class ServerCreationTest : IClassFixture<WebApplication>
    {
        private readonly WebApplication _factory;

        public ServerCreationTest(WebApplication factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Create()
        {
            var client = _factory.CreateClient();

            using (var root = await client.GetAsync("/"))
            {
                Assert.Equal(HttpStatusCode.NotFound, root.StatusCode);
            }

            await _factory.RunScoped(async sp =>
            {
                var confs = sp.GetRequiredService<IConfigurationRegistry>();
                await confs.UpdateAsync("random_count", "5");
            });

            using (var weatherGet = await client.GetAsync("/api/weather"))
            {
                var content = await weatherGet.Content.ReadAsJsonAsync<List<WeatherForecast>>();
                Assert.Equal(5, content.Count);
            }

            await _factory.RunScoped(async sp =>
            {
                var confs = sp.GetRequiredService<IConfigurationRegistry>();
                await confs.UpdateAsync("random_count", "10");
            });

            using (var weatherGet = await client.GetAsync("/api/weather"))
            {
                var content = await weatherGet.Content.ReadAsJsonAsync<List<WeatherForecast>>();
                Assert.Equal(10, content.Count);
            }
        }
    }
}
