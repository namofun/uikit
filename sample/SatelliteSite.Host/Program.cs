using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SatelliteSite.IdentityModule.Entities;
using SatelliteSite.Services;

namespace SatelliteSite
{
    public class Program
    {
        public static IHost Current { get; private set; }

        public static void Main(string[] args)
        {
            Current = CreateHostBuilder(args).Build();
            Current.AutoMigrate<DefaultContext>();
            Current.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .MarkDomain<Program>()
                .AddModule<IdentityModule.IdentityModule<User, Role, DefaultContext>>()
                .AddModule<SampleModule.SampleModule>()
                .AddModule<TelemetryModule.TelemetryModule>()
                .AddDatabase<DefaultContext>((c, b) => b.UseSqlServer(c.GetConnectionString("UserDbConnection"), b => b.UseBulk()))
                .ConfigureSubstrateDefaults<DefaultContext>(builder =>
                {
                    builder.ConfigureServices((ctx, services) =>
                    {
                        services.ConfigureIdentityAdvanced(options =>
                        {
                            options.ExternalLogin = true;
                            options.TwoFactorAuthentication = true;
                            options.ShortenedClaimName = true;
                        });

                        services.Configure<ApplicationInsightsDisplayOptions>(options =>
                        {
                            options.ApiKey = ctx.Configuration["AppInsights:Key"] ?? "DEMO_KEY";
                            options.ApplicationId = ctx.Configuration["AppInsights:App"] ?? "DEMO_APP";
                        });

                        services.Configure<Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions>(options =>
                        {
                            if (string.IsNullOrEmpty(options.InstrumentationKey))
                            {
                                options.InstrumentationKey = ctx.Configuration["AppInsights:App"] ?? string.Empty;
                            }
                        });

                        if (ctx.Configuration["AzureAD:ClientId"] != null)
                        {
                            AzureAdAuthentication.AddAzureAd(new AuthenticationBuilder(services), options =>
                            {
                                options.Instance = ctx.Configuration["AzureAD:Instance"];
                                options.Domain = ctx.Configuration["AzureAD:Domain"];
                                options.ClientId = ctx.Configuration["AzureAD:ClientId"];
                                options.ClientSecret = ctx.Configuration["AzureAD:ClientSecret"];
                                options.TenantId = ctx.Configuration["AzureAD:TenantId"];
                            });
                        }
                    });
                });
    }
}
