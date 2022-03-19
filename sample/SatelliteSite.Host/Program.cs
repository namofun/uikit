using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public static IHostBuilder CreateHostBuilderAlt(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .MarkDomain<Program>()
                .AddApplicationInsights()
                .AddModule<AzureCloud.EasyAuthModule>()
                .AddModule<SampleModule.SampleModule>()
                .AddAzureBlobWebRoot((c, options) => options.With(c.GetConnectionString("AzureStorageBlob"), c.GetConnectionString("AzureStorageBlobContainerName"), System.IO.Path.Combine(c.HostingEnvironment.ContentRootPath, "wwwcache")))
                .ConfigureSubstrateDefaultsCore(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddAuthentication("EasyAuth");
                        services.AddSingleton<ITelemetryInitializer, LogicAppsInitializer>();
                    });
                });

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .MarkDomain<Program>()
                .AddApplicationInsights()
                .AddModule<IdentityModule.IdentityModule<User, Role, DefaultContext>>()
                .EnableIdentityModuleBasicAuthentication()
                .AddModule<SampleModule.SampleModule>()
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

                        services.ConfigureApplicationBuilder(options =>
                        {
                            options.GravatarMirror = "//gravatar.zeruns.tech/avatar/";
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

                        services.AddSingleton<ITelemetryInitializer, LogicAppsInitializer>();
                    });
                });
    }
}
