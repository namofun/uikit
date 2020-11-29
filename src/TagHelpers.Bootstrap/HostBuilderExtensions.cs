using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Menus;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SatelliteSite.Entities;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;

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
        public static IApplicationBuilder UseFakeAuthorization(this IApplicationBuilder app)
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
        /// Add a module and configure them in the next constructing pipeline.
        /// </summary>
        /// <typeparam name="TModule">The only <see cref="AbstractModule"/> in that Assembly</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <param name="convention">The conventions for all endpoints</param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder AddModule<TModule>(this IHostBuilder builder, Action<IEndpointConventionBuilder> convention) where TModule : AbstractModule, new()
        {
            ((List<AbstractModule>)Startup.Modules).Add(new TModule { Conventions = convention });
            return builder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IWebHostBuilder"/> class with pre-configured defaults.
        /// </summary>
        /// <remarks>
        /// The following defaults are applied to the <see cref="IWebHostBuilder"/>: <br />
        /// - Use Kestrel as the web server and configure it using the application's configuration providers. <br />
        /// - Adds the HostFiltering middleware. <br />
        /// - Adds the ForwardedHeaders middleware if <c>ASPNETCORE_FORWARDEDHEADERS_ENABLED</c>=true. <br />
        /// - Enable IIS integration. <br />
        /// - Use the moduled built-in <see cref="Startup"/> configurations.
        /// </remarks>
        /// <typeparam name="TContext">The default core service <see cref="DbContext"/>.</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/> instance to configure.</param>
        /// <param name="further">The configure callback.</param>
        /// <returns>The <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder ConfigureSubstrateDefaults<TContext>(
            this IHostBuilder builder,
            Action<IWebHostBuilder>? further = null)
            where TContext : DbContext
        {
            if (DataAccessExtensions.MigrationAssembly == null)
                throw new ArgumentNullException("The migration assembly is invalid.");
            further ??= _ => { };

            // register webservices
            return builder.ConfigureWebHostDefaults(builder =>
            {
                builder.UseStaticWebAssets();
                builder.UseStartup<Startup>();
                builder.UseSetting(WebHostDefaults.ApplicationKey, DataAccessExtensions.MigrationAssembly);

                builder.ConfigureServices((context, services) =>
                {
                    // auditlogger and configuration registry
                    services.AddScoped<IAuditlogger, Auditlogger<TContext>>();
                    services.AddScoped<IConfigurationRegistry, ConfigurationRegistry<TContext>>();
                    services.AddDbModelSupplier<TContext, CoreEntityConfiguration<TContext>>();

                    // module services
                    var menuContributor = new ConcreteMenuContributor();
                    menuContributor.ConfigureDefaults();

                    foreach (var module in Startup.Modules)
                    {
                        var type = typeof(ModuleEndpointDataSource<>).MakeGenericType(module.GetType());
                        services.AddSingleton(type);
                        module.RegisterServices(services, context.Configuration, context.HostingEnvironment);
                        module.RegisterMenu(menuContributor);
                    }

                    menuContributor.Contribute();
                    services.AddSingleton<IMenuProvider>(menuContributor);
                });

                further.Invoke(builder);
            });
        }

        /// <summary>
        /// Apply the substrate endpoint into the route builder.
        /// </summary>
        /// <param name="builder">The route builder</param>
        /// <returns>The route builder</returns>
        internal static void MapSubstrate(this IEndpointRouteBuilder builder)
        {
            builder.DataSources.Add(new RootEndpointDataSource(builder.ServiceProvider));
        }

        /// <summary>
        /// Apply the endpoint modules into the route builder.
        /// </summary>
        /// <param name="builder">The route builder</param>
        /// <param name="modules">The endpoint configuration list</param>
        /// <returns>The route builder</returns>
        internal static void MapModules(this IEndpointRouteBuilder builder, IReadOnlyCollection<AbstractModule> modules)
        {
            foreach (var module in modules)
            {
                module.RegisterEndpoints(ModuleEndpointDataSourceBase.Factory(module, builder));
            }
        }

        /// <summary>
        /// Apply the re-execute endpoint into the route builder.
        /// </summary>
        /// <param name="builder">The route builder</param>
        /// <returns>The route builder</returns>
        internal static void MapReExecute(this IEndpointRouteBuilder builder)
        {
            builder.ServiceProvider
                .GetRequiredService<ReExecuteEndpointMatcher>()
                .BuildPassOne();
        }

        /// <summary>
        /// Conventions for endpoints that requires the authorization.
        /// </summary>
        /// <param name="builder">The endpoint convention builder.</param>
        /// <param name="roles">The roles to be confirmed.</param>
        /// <returns>The endpoint convention builder to chain the configurations.</returns>
        public static IEndpointConventionBuilder RequireRoles(this IEndpointConventionBuilder builder, string roles)
        {
            return builder.RequireAuthorization(new AuthorizeAttribute { Roles = roles });
        }

        /// <summary>
        /// Conventions for endpoints that changes the name.
        /// </summary>
        /// <param name="builder">The endpoint convention builder.</param>
        /// <param name="configure">The name of that endpoint.</param>
        /// <returns>The endpoint convention builder to chain the configurations.</returns>
        public static IEndpointConventionBuilder WithDisplayName(this IEndpointConventionBuilder builder, Func<string, string> configure)
        {
            builder.Add(b => b.DisplayName = configure.Invoke(b.DisplayName));
            return builder;
        }

        /// <summary>
        /// Conventions in default.
        /// </summary>
        /// <param name="builder">The endpoint convention builder.</param>
        /// <param name="configure">The configure delegate.</param>
        /// <returns>The endpoint convention builder to chain the configurations.</returns>
        internal static TEndpointConventionBuilder WithDefaults<TEndpointConventionBuilder>(this TEndpointConventionBuilder builder, Action<IEndpointConventionBuilder> configure) where TEndpointConventionBuilder : IEndpointConventionBuilder
        {
            configure.Invoke(builder);
            return builder;
        }
    }
}
