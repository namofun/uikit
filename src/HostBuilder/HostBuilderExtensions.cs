﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Menus;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SatelliteSite.Substrate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Provide several extension methods to add modules.
    /// </summary>
    public static class HostBuilderModulingExtensions
    {
        /// <summary>
        /// Adds a fake middleware to the specified <see cref="IApplicationBuilder"/>,
        /// which adds <c>__AuthorizationMiddlewareWithEndpointInvoked</c> to items so that
        /// <see cref="EndpointMiddleware"/> do not throw an exception.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        internal static IApplicationBuilder UseFakeAuthorization(this IApplicationBuilder app)
        {
            app.ApplicationServices
                .GetRequiredService<ILogger<Startup>>()
                .LogWarning("No modules providing identity feature. Fake authorization will be used.");

            return app.Use((httpContext, next) =>
            {
                httpContext.Items["__AuthorizationMiddlewareWithEndpointInvoked"] = true;
                return next();
            });
        }

        /// <summary>
        /// Adds all extensions to the specified <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="actions">The configure action list.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        internal static IApplicationBuilder UseExtensions(this IApplicationBuilder app, List<Action<IApplicationBuilder>> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].Invoke(app);
            }

            return app;
        }

        /// <summary>
        /// Continue the mvc builder configure with the following action.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="setupAction">The setup action.</param>
        /// <returns>The mvc builder itself.</returns>
        internal static IMvcBuilder ContinueWith(this IMvcBuilder builder, Action<IMvcBuilder> setupAction)
        {
            setupAction(builder);
            return builder;
        }

        /// <summary>
        /// Checks if a given Url matches rules and conditions, and modifies the HttpContext on match.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The original application builder</returns>
        public static IApplicationBuilder UseUrlRewriting(this IApplicationBuilder app)
        {
            if (app.ApplicationServices.GetRequiredService<IUrlHelperFactory>() is SubstrateUrlHelperFactory urlHelperFactory)
            {
                if (urlHelperFactory.RewriteRules.Count > 0)
                {
                    urlHelperFactory.Enabled = true;
                    var options = new RewriteOptions();
                    foreach (var rule in urlHelperFactory.RewriteRules)
                    {
                        options.Add(rule);
                    }

                    app.UseRewriter(options);
                }

                return app;
            }

            throw new InvalidOperationException("IUrlHelperFactory is not compatible with current configurations.");
        }

        /// <summary>
        /// Add a module and configure them in the next constructing pipeline.
        /// </summary>
        /// <typeparam name="TModule">The only <see cref="AbstractModule"/> in that Assembly</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder AddModule<TModule>(this IHostBuilder builder) where TModule : AbstractModule, new()
        {
            return AddModule<TModule>(builder, _ => { });
        }

        /// <summary>
        /// Set the current culture info.
        /// </summary>
        /// <param name="builder">The host builder.</param>
        /// <param name="cultureName">The name of culture.</param>
        public static IHostBuilder WithCultureInfo(this IHostBuilder builder, string cultureName)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            return builder;
        }

        /// <summary>
        /// Add a module and configure them in the next constructing pipeline.
        /// </summary>
        /// <typeparam name="TModule">The only <see cref="AbstractModule"/> in that Assembly</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <param name="convention">The conventions for all endpoints</param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder AddModule<TModule>(this IHostBuilder builder, Action<IEndpointConventionBuilder> convention) where TModule : AbstractModule, new()
        {
            builder.Modules().Add(new TModule { Conventions = convention });
            return builder;
        }

        /// <summary>
        /// Add a rewrite rule and configure them in the next constructing pipeline.
        /// </summary>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <param name="rewriteRule">The <see cref="IRewriteRule"/></param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder AddRewriteRule(this IHostBuilder builder, IRewriteRule rewriteRule)
        {
            builder.ConfigureServices(services => services.AddSingleton(rewriteRule));
            return builder;
        }

        /// <summary>
        /// Gets the list of mutable modules.
        /// </summary>
        /// <param name="builder">The host builder.</param>
        /// <returns>The list for modules.</returns>
        private static List<AbstractModule> Modules(this IHostBuilder builder)
        {
            if (!builder.Properties.ContainsKey("Substrate.Modules"))
            {
                builder.Properties.Add("Substrate.Modules", new List<AbstractModule>());
            }

            return (List<AbstractModule>)builder.Properties["Substrate.Modules"];
        }

        /// <summary>
        /// Configure the <see cref="WebHostBuilderContext.HostingEnvironment"/> to be <see cref="SubstrateEnvironment"/>.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="modules">The modules.</param>
        /// <returns>The builder.</returns>
        private static IWebHostBuilder ConfigureEnvironment(this IWebHostBuilder builder, IReadOnlyCollection<AbstractModule> modules)
        {
            return builder.ConfigureAppConfiguration((ctx, cb) =>
            {
                ctx.HostingEnvironment = new SubstrateEnvironment(ctx.HostingEnvironment, modules);
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IWebHostBuilder"/> class with pre-configured defaults.
        /// </summary>
        /// <remarks>
        /// The following defaults are applied to the <see cref="IWebHostBuilder"/>:
        /// <list type="bullet">Use Kestrel as the web server and configure it using the application's configuration providers.</list>
        /// <list type="bullet">Adds the HostFiltering middleware.</list>
        /// <list type="bullet">Adds the ForwardedHeaders middleware if <c>ASPNETCORE_FORWARDEDHEADERS_ENABLED</c>=true.</list>
        /// <list type="bullet">Enable IIS integration.</list>
        /// <list type="bullet">Use the moduled built-in <see cref="Startup"/> configurations.</list>
        /// </remarks>
        /// <typeparam name="TModule">The default core module <see cref="SatelliteSite.Substrate.DefaultModule"/>.</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/> instance to configure.</param>
        /// <param name="further">The configure callback.</param>
        /// <returns>The <see cref="IHostBuilder"/> for chaining.</returns>
        private static IHostBuilder ConfigureSubstrateWithDefaultModule<TModule>(
            this IHostBuilder builder,
            Action<IWebHostBuilder>? further = null)
            where TModule : DefaultModule, new()
        {
            bool hasDomainSpecified = builder.GetDomain(out var domainName);
            further ??= _ => { };

            var modules = builder.Modules();
            modules.Insert(0, new TModule());
            foreach (var module in modules)
            {
                module.Initialize();
            }

            // register webservices
            return builder.ConfigureWebHostDefaults(builder =>
            {
                builder.UseStaticWebAssets();
                builder.ConfigureEnvironment(modules);
                builder.UseStartup<Startup>();

                if (hasDomainSpecified)
                {
                    builder.UseSetting(WebHostDefaults.ApplicationKey, domainName);
                }

                builder.ConfigureServices((context, services) =>
                {
                    services.AddSingleton(new ReadOnlyCollection<AbstractModule>(modules));
                    services.AddSingleton<ConcreteMenuContributor>();
                    services.AddSingletonUpcast<IMenuProvider, ConcreteMenuContributor>();

                    foreach (var module in modules)
                    {
                        services.AddSingleton(typeof(ModuleEndpointDataSource<>).MakeGenericType(module.GetType()));
                        module.RegisterServices(services, context.Configuration, context.HostingEnvironment);
                    }
                });

                further.Invoke(builder);
            });
        }

        /// <inheritdoc cref="ConfigureSubstrateWithDefaultModule{TModule}(IHostBuilder, Action{IWebHostBuilder}?)"/>
        /// <summary>
        /// Initializes a new instance of the <see cref="IWebHostBuilder"/> class with pre-configured defaults,
        /// but without auditlogger and configuration registry.
        /// </summary>
        public static IHostBuilder ConfigureSubstrateDefaultsCore(
            this IHostBuilder builder,
            Action<IWebHostBuilder>? further = null)
            => ConfigureSubstrateWithDefaultModule<DefaultModule>(builder, further);

        /// <inheritdoc cref="ConfigureSubstrateWithDefaultModule{TModule}(IHostBuilder, Action{IWebHostBuilder}?)"/>
        /// <typeparam name="TContext">The default core service <see cref="DbContext"/>.</typeparam>
        public static IHostBuilder ConfigureSubstrateDefaults<TContext>(
            this IHostBuilder builder,
            Action<IWebHostBuilder>? further = null)
            where TContext : DbContext
            => ConfigureSubstrateWithDefaultModule<DefaultModule<TContext>>(builder, further);
    }
}
