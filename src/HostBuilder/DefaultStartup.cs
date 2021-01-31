﻿using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SatelliteSite.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Default startup class to configure the services and request pipeline.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Create an instance of <see cref="Startup"/>
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/></param>
        /// <param name="env">The <see cref="IWebHostEnvironment"/></param>
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
            Modules = ((ISubstrateEnvironment)env).Modules;
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
        /// The modules to configure
        /// </summary>
        public IReadOnlyCollection<AbstractModule> Modules { get; }

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
            services.TryAddSingleton<ITelemetryClient, NullTelemetryClient>();

            services.AddMediatR(Modules.Select(a => a.GetType().Assembly).ToArray());

            services.AddWebEncoders(options =>
            {
                options.TextEncoderSettings ??= new TextEncoderSettings();
                options.TextEncoderSettings.AllowRange(UnicodeRanges.BasicLatin);
                options.TextEncoderSettings.AllowRange(UnicodeRanges.CjkUnifiedIdeographs);
            });

            services.AddSingleton<ReExecuteEndpointDataSource>();
            services.AddSingletonDowncast<CompositeEndpointDataSource, EndpointDataSource>();
            services.AddSingleton<ReExecuteEndpointMatcher>();
            services.ReplaceSingleton<LinkGenerator, OrderLinkGenerator>();
            services.ReplaceSingleton<IUrlHelperFactory, SubstrateUrlHelperFactory>();

            services.AddControllersWithViews()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new TimeSpanJsonConverter()))
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .ContinueWith(ConfigureParts);

            services.AddSingleton<SubstrateApiVisibilityConvention>();
            services.ConfigureOptions<SubstrateMvcOptionsConfigurator>();
            services.ReplaceSingleton<ITempDataProvider, CompositeTempDataProvider>();

            if (!string.IsNullOrWhiteSpace(Environment.WebRootPath))
                services.AddSingleton<IWwwrootFileProvider, WwwrootFileProvider>();

            services.AddSession(options => options.Cookie.IsEssential = true);

            services.AddApiExplorer(o => o.DocInclusionPredicate((a, b) => b.GroupName == a))
                .AddSecurityScheme("basic", Microsoft.OpenApi.Models.SecuritySchemeType.Http);
            services.AddSingleton<IApiDocumentProvider, ApiDocumentProvider>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="options">The substrate options</param>
        public void Configure(IApplicationBuilder app, IOptions<SubstrateOptions> options)
        {
            app.EnsureClaimTypes();

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMiddleware<AjaxExceptionMiddleware>();
                app.UseDatabaseErrorPage();
                app.UseStatusCodePage();
            }
            else
            {
                app.UseHttpsRedirection();
                app.UseExceptionHandler("/error");
                app.UseStatusCodePage();
                app.UseCatchException();
                app.UseHsts();
            }

            app.UseExtensions(options.Value.PointBeforeUrlRewriting);
            app.UseStaticFiles();
            app.UseUrlRewriting();

            app.UseExtensions(options.Value.PointBeforeRouting);
            app.UseCookiePolicy();
            app.UseSession();

            app.UseRouting();
            app.UseCors();

            if (Modules.Any(m => m.ProvideIdentity))
            {
                app.UseAuthentication();
                app.UseExtensions(options.Value.PointBetweenAuth);
                app.UseAuthorization();
            }
            else if (Environment.IsDevelopment())
            {
                app.UseFakeAuthorization();
            }

            app.UseExtensions(options.Value.PointBeforeEndpoint);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapModules(Modules);
                endpoints.MapReExecute();
                endpoints.MapExtensions(options.Value.Endpoints);
            });
        }
    }
}
