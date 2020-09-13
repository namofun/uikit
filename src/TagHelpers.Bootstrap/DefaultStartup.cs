using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Default startup class to configure the services and request pipeline.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The databases to configure
        /// </summary>
        internal static ICollection<Action<IServiceCollection, IConfiguration>> Databases { get; } = new List<Action<IServiceCollection, IConfiguration>>();

        /// <summary>
        /// The modules to configure
        /// </summary>
        internal static ICollection<AbstractModule> Modules { get; } = new List<AbstractModule>();

        /// <summary>
        /// Create an instance of <see cref="Startup"/>
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/></param>
        /// <param name="env">The <see cref="IWebHostEnvironment"/></param>
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        /// <summary>
        /// The configuration of running application
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// The environment with web host specified settings
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The dependency injection builder</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddApplicationInsightsTelemetry();

            services.AddMediatR(Modules.Select(a => a.GetType().Assembly).ToArray());

            foreach (var items in Databases)
                items.Invoke(services, Configuration);

            /*
            if (Configuration["IdentityServer:Enabled"] == "True")
                services.AddIdentityServer()
                    .AddAspNetIdentity<User>()
                    .AddInMemoryClients(Configuration.GetSection("IdentityServer:Clients"))
                    .AddInMemoryIdentityResources(Configuration.GetSection("IdentityServer:Scopes"))
                    .AddInMemoryApiResources(Configuration.GetSection("IdentityServer:Apis"))
                    .AddDeveloperSigningCredential();
            */

            services.AddSingleton(
                HtmlEncoder.Create(
                    UnicodeRanges.BasicLatin,
                    UnicodeRanges.CjkUnifiedIdeographs));

            Modules.ApplyServices(services, Configuration);

            services.AddControllersWithViews()
                .AddTimeSpanJsonConverter()
                .UseSlugifyParameterTransformer()
                .ReplaceDefaultLinkGenerator()
                .AddSessionStateTempDataProvider()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddOnboardingModules(Modules, Environment.IsDevelopment());

            services.AddSingleton<ReExecuteEndpointMatcher>();

            services.AddSession(options => options.Cookie.IsEssential = true);

            services.AddApiExplorer(o => o.DocInclusionPredicate((a, b) => b.GroupName == a))
                .AddSecurityScheme("basic", Microsoft.OpenApi.Models.SecuritySchemeType.Http);
        }


        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder</param>
        public void Configure(IApplicationBuilder app)
        {
            app.EnsureClaimTypes();

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMiddleware<AjaxExceptionMiddleware>();
                //app.UseDatabaseErrorPage();
                app.UseStatusCodePage();
            }
            else if (Environment.EnvironmentName == "Test")
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

                app.UseDeveloperExceptionPage();
                app.UseMiddleware<AjaxExceptionMiddleware>();
                //app.UseDatabaseErrorPage();
                app.UseStatusCodePage();
            }
            else
            {
                //app.UseMiddleware<RealIpMiddleware>();
                app.UseHttpsRedirection();
                app.UseExceptionHandler("/error");
                app.UseStatusCodePage();
                app.UseCatchException();
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();

            //if (Configuration["IdentityServer:Enabled"] == "True")
            //    app.UseIdentityServer();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSubstrate();
                endpoints.MapModules(Modules);

                endpoints.MapNotFound("/api/{**slug}");
                endpoints.MapNotFound("/lib/{**slug}");
                endpoints.MapNotFound("/images/{**slug}");

                endpoints.MapReExecute();
            });
        }
    }
}
