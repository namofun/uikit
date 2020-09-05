using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Menus;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SatelliteSite.Entities;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
        /// Apply the endpoint modules into the route builder.
        /// </summary>
        /// <param name="builder">The route builder</param>
        /// <param name="modules">The endpoint configuration list</param>
        /// <returns>The route builder</returns>
        internal static void ApplyEndpoints(this ICollection<AbstractModule> modules, IEndpointRouteBuilder builder)
        {
            foreach (var module in modules)
            {
                module.RegisterEndpoints(ModuleEndpointDataSourceBase.Factory(module, builder));
            }
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

        /// <summary>
        /// Apply the application parts into the manager.
        /// </summary>
        /// <param name="apm">The application part manager</param>
        /// <param name="modules">The application parts list</param>
        /// <returns>The application part manager</returns>
        internal static (List<ApplicationPart>, PeerFileProvider?) GetParts(this ICollection<AbstractModule> modules)
        {
            var lst = new List<ApplicationPart>();
            PeerFileProvider? tree = null;

            static bool TryLoad(string assemblyName, out Assembly? assembly)
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + assemblyName))
                {
                    assembly = Assembly.LoadFrom(AppDomain.CurrentDomain.BaseDirectory + assemblyName);
                    return true;
                }
                else
                {
                    assembly = null;
                    return false;
                }
            }

            void Add(Assembly assembly, string areaName)
            {
                var assemblyName = assembly.GetName().Name;
                if (string.IsNullOrEmpty(assemblyName))
                    throw new TypeLoadException("The assembly is invalid.");

                if (!assemblyName!.EndsWith(".Views"))
                {
                    lst.Add(new AssemblyPart(assembly));
                    var rdpa = assembly.GetCustomAttribute<LocalDebugPathAttribute>();
                    if (rdpa != null)
                    {
                        tree ??= new PeerFileProvider();
                        var dir1 = Path.Combine(rdpa.Path, "Views");
                        if (Directory.Exists(dir1))
                            tree["Areas"][areaName]["Views"].Append(new PhysicalFileProvider(dir1));
                        var dir2 = Path.Combine(rdpa.Path, "Panels");
                        if (Directory.Exists(dir2))
                            tree["Areas"]["Dashboard"]["Views"].Append(new PhysicalFileProvider(dir2));
                        var dir3 = Path.Combine(rdpa.Path, "Components");
                        if (Directory.Exists(dir3))
                            tree["Views"]["Components"].Append(new PhysicalFileProvider(dir3));
                    }
                }
                else
                {
                    lst.Add(new ViewsAssemblyPart(assembly, areaName));
                }

                foreach (var rel in assembly.GetCustomAttributes<RelatedAssemblyAttribute>())
                {
                    if (AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == rel.AssemblyFileName).Any()) return;
                    if (!TryLoad(rel.AssemblyFileName + ".dll", out var ass))
                        throw new TypeLoadException("The assembly is invalid.");
                    Add(ass!, areaName);
                }
            }

            var selfCheck = typeof(AbstractModule).Assembly
                .GetCustomAttribute<LocalDebugPathAttribute>();
            if (selfCheck != null)
                (tree ??= new PeerFileProvider()).Append(new PhysicalFileProvider(selfCheck.Path));
            
            foreach (var module in modules)
                Add(module.GetType().Assembly, module.Area);
            return (lst, tree);
        }
    }
}
