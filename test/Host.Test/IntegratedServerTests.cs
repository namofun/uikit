using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.SampleModule.Models;
using SatelliteSite.Services;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SatelliteSite.Tests
{
    public class IntegratedServerTests : IClassFixture<WebApplication>
    {
        private readonly WebApplication _factory;

        public IntegratedServerTests(WebApplication factory)
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

        [Fact]
        public async Task EndpointBuilding()
        {
            var client = _factory.CreateClient();

            using (var root = await client.GetAsync("/"))
            {
                var body = await root.Content.ReadAsStringAsync();
                Assert.Equal(string.Empty, body);
                Assert.Equal(HttpStatusCode.NotFound, root.StatusCode);
            }

            using (var root = await client.GetAsync("/weather/checker"))
            {
                var body = await root.Content.ReadAsStringAsync();
                Assert.Equal("Hello World!", body);
                Assert.Equal(HttpStatusCode.OK, root.StatusCode);
            }

            using (var root = await client.GetAsync("/api/doc/sample"))
            {
                var body = await root.Content.ReadAsStringAsync();
                Assert.Contains("<script id=\"swagger-data\" type=\"application/json\">", body);
                Assert.Equal(HttpStatusCode.OK, root.StatusCode);
            }

            using (var root = await client.GetAsync("/sample/world"))
            {
                var body = await root.Content.ReadAsStringAsync();
                Assert.Contains("<div class=\"error-template\">", body);
                Assert.Contains("402 Payment Required", body);
                Assert.Equal(HttpStatusCode.PaymentRequired, root.StatusCode);
            }

            using (var root = await client.GetAsync("/sample/not-found-easy"))
            {
                var body = await root.Content.ReadAsStringAsync();
                Assert.Contains("<div class=\"error-template\">", body);
                Assert.Contains("404 Not Found", body);
                Assert.Equal(HttpStatusCode.NotFound, root.StatusCode);
            }

            using (var root = await client.DeleteAsync("/api/weather/1"))
            {
                Assert.Equal(HttpStatusCode.NoContent, root.StatusCode);
            }
        }

        [Fact]
        public async Task LoginSlideExpiration()
        {
            var client = _factory.CreateClient(s => s.AllowAutoRedirect = false);
            const string username = "test";
            const string password = "123456";

            await _factory.RunScoped(async sp =>
            {
                var um = sp.GetRequiredService<IUserManager>();
                var newUser = um.CreateEmpty(username);
                newUser.Email = "test@test.com";
                var id = await um.CreateAsync(newUser, password);
                Assert.True(id.Succeeded);
            });

            Assert.True(await client.LoginAsync(username, password));

            using (var root = await client.GetAsync("/sample/main/claims?roleName=Administrator"))
            {
                Assert.False(await root.Content.ReadAsJsonAsync<bool>());
            }

            await _factory.RunScoped(async sp =>
            {
                var um = sp.GetRequiredService<IUserManager>();
                var user = await um.FindByNameAsync(username);
                var id = await um.AddToRoleAsync(user, "Administrator");
                Assert.True(id.Succeeded);
            });

            using (var root = await client.GetAsync("/sample/main/claims?roleName=Administrator"))
            {
                Assert.True(await root.Content.ReadAsJsonAsync<bool>());
            }

            await _factory.RunScoped(async sp =>
            {
                var um = sp.GetRequiredService<IUserManager>();
                var user = await um.FindByNameAsync(username);
                var id = await um.ChangePasswordAsync(user, password, "newpassword");
                Assert.True(id.Succeeded);
            });

            using (var root = await client.GetAsync("/sample/main/claims?roleName=Administrator"))
            {
                // login auto redirect
                Assert.Equal(HttpStatusCode.Found, root.StatusCode);
            }
        }
    }
}
