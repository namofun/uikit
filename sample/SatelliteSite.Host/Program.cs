using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SatelliteSite.IdentityModule.Entities;

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
                .AddDatabaseMssql<DefaultContext>("UserDbConnection")
                .ConfigureSubstrateDefaults<DefaultContext>(builder =>
                {
                    builder.ConfigureServices((ctx, services) =>
                    {
                        services.Configure<IdentityAdvancedOptions>(options =>
                        {
                            options.ExternalLogin = true;
                            options.TwoFactorAuthentication = true;
                        });

                        new AuthenticationBuilder(services)
                            .AddAzureAd(options =>
                            {
                                options.Instance = ctx.Configuration["AzureAD:Instance"];
                                options.Domain = ctx.Configuration["AzureAD:Domain"];
                                options.ClientId = ctx.Configuration["AzureAD:ClientId"];
                                options.ClientSecret = ctx.Configuration["AzureAD:ClientSecret"];
                                options.TenantId = ctx.Configuration["AzureAD:TenantId"];
                            });
                    });
                });
    }
}
