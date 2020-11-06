﻿using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        /// The modules to configure
        /// </summary>
        public static IReadOnlyCollection<AbstractModule> Modules { get; } = new List<AbstractModule>();

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
        /// AssemblyLoadFileDelegate for <see cref="ApplicationParts.RelatedAssemblyAttribute"/>.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The loaded assembly.</returns>
        private static Assembly AssemblyLoadFileDelegate(string fileName)
        {
            bool AreSameAssembly(Assembly a) => !a.IsDynamic && string.Equals(a.Location, fileName, StringComparison.OrdinalIgnoreCase);

            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(AreSameAssembly);
            if (assembly != null) return assembly;
            return Assembly.LoadFile(fileName);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The dependency injection builder</param>
        public void ConfigureServices(IServiceCollection services)
        {
            typeof(ApplicationParts.RelatedAssemblyAttribute)
                .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .Single(f => f.Name == nameof(AssemblyLoadFileDelegate))
                .SetValue(null, new Func<string, Assembly>(AssemblyLoadFileDelegate));

            services.AddMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddApplicationInsightsTelemetry();

            var assemblies = Modules.Select(a => a.GetType().Assembly).ToArray();
            if (assemblies.Length > 0)
                services.AddMediatR(assemblies);

            services.AddSingleton(
                HtmlEncoder.Create(
                    UnicodeRanges.BasicLatin,
                    UnicodeRanges.CjkUnifiedIdeographs));

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

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();

            app.UseRouting();

            if (Modules.Any(m => m.ProvideIdentity))
                app.UseAuthentication()
                   .UseAuthorization();
            else if (Environment.IsDevelopment())
                app.UseFakeAuthorization();

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
