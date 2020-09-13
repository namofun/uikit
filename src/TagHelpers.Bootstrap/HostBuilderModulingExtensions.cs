using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Menus;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        /// The name of migration assembly
        /// </summary>
        private static string? MigrationAssembly { get; set; }

        /// <summary>
        /// Mark the application domain.
        /// </summary>
        /// <typeparam name="T">The program class.</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder MarkDomain<T>(this IHostBuilder builder)
        {
            MigrationAssembly
                = typeof(T).Assembly.GetName().Name
                ?? throw new ArgumentNullException("The migration assembly is invalid.");
            return builder;
        }

        /// <summary>
        /// Add a module and configure them in the next constructing pipeline.
        /// </summary>
        /// <typeparam name="TModule">The only <see cref="AbstractModule"/> in that Assembly</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder AddModule<TModule>(this IHostBuilder builder) where TModule : AbstractModule, new()
        {
            Startup.Modules.Add(new TModule());
            return builder;
        }

        /// <summary>
        /// Add a <see cref="DbContext"/> and configure them in the next constructing pipeline.
        /// </summary>
        /// <typeparam name="TContext">The required <see cref="DbContext"/>.</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <param name="configures">The configure delegate</param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder AddDatabase<TContext>(
            this IHostBuilder builder,
            Action<IConfiguration, DbContextOptionsBuilder> configures) where TContext : DbContext
        {
            Startup.Databases.Add((services, conf) =>
            {
                services.AddDbContext<TContext>(options =>
                {
                    configures.Invoke(conf, options);
                });
            });

            return builder;
        }

        /// <summary>
        /// Add a <see cref="DbContext"/> and configure them in the next constructing pipeline.
        /// </summary>
        /// <typeparam name="TContext">The required <see cref="DbContext"/>.</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <param name="connectionStringName">The connection string name</param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder AddDatabaseMssql<TContext>(
            this IHostBuilder builder,
            string connectionStringName) where TContext : DbContext
        {
            _ = MigrationAssembly ?? throw new ArgumentNullException("The migration assembly is invalid.");
            return builder.AddDatabase<TContext>((conf, opt) =>
            {
                opt.UseSqlServer(
                    conf.GetConnectionString(connectionStringName),
                    o => o.MigrationsAssembly(MigrationAssembly));
                opt.UseBulkExtensions();
            });
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
            _ = MigrationAssembly ?? throw new ArgumentNullException("The migration assembly is invalid.");

            Startup.Databases.Add((services, configuration) =>
            {
                services.AddScoped<IAuditlogger, Auditlogger<TContext>>();
                services.AddScoped<IConfigurationRegistry, ConfigurationRegistry<TContext>>();
                services.AddDbModelSupplier<TContext, CoreEntityConfiguration<TContext>>();
            });

            return builder.ConfigureWebHostDefaults(builder =>
            {
                builder.UseStaticWebAssets();
                builder.UseStartup<Startup>();
                builder.UseSetting(WebHostDefaults.ApplicationKey, MigrationAssembly);
                further?.Invoke(builder);
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
        internal static void MapModules(this IEndpointRouteBuilder builder, ICollection<AbstractModule> modules)
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
        /// Apply the dependency into the dependency injection builder.
        /// </summary>
        /// <param name="builder">The dependency injection builder</param>
        /// <param name="modules">The dependency configuration list</param>
        /// <param name="configuration">The configuration source</param>
        internal static void ApplyServices(this ICollection<AbstractModule> modules, IServiceCollection builder, IConfiguration configuration)
        {
            var menuContributor = new ConcreteMenuContributor();
            menuContributor.ConfigureDefaults();

            foreach (var module in modules)
            {
                var type = typeof(ModuleEndpointDataSource<>).MakeGenericType(module.GetType());
                builder.AddSingleton(type);
                module.RegisterServices(builder, configuration);
                module.RegisterMenu(menuContributor);
            }

            menuContributor.Contribute();
            builder.AddSingleton<IMenuProvider>(menuContributor);
        }
    }
}
