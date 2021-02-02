using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Data access extensions for Substrate.
    /// </summary>
    public static class HostBuilderDataAccessExtensions
    {
        /// <summary>
        /// The key for application domain indicating.
        /// </summary>
        public const string ApplicationDomain = "Substrate.ApplicationDomain";

        /// <summary>
        /// An exception description for checking bulk extensions.
        /// </summary>
        public const string NoBulkExtRegistered =
            "Extensions for Microsoft.EntityFrameworkCore.Bulk hasn't been registered. " +
            "Please register it with options.UseSqlServer(..., b => b.UseBulk()).";

        /// <summary>
        /// Mark the application domain to prevent problems.
        /// </summary>
        /// <typeparam name="T">The program class.</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder MarkDomain<T>(this IHostBuilder builder)
        {
            builder.Properties[ApplicationDomain]
                =  typeof(T).Assembly.GetName().Name
                ?? throw new ArgumentNullException("The migration assembly is invalid.");
            return builder;
        }

        /// <summary>
        /// Get the application domain information.
        /// </summary>
        /// <param name="builder">The host builder.</param>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>Whether application domain is used.</returns>
        internal static bool GetDomain(this IHostBuilder builder, out string? assemblyName)
        {
            if (!builder.Properties.TryGetValue(ApplicationDomain, out var ma))
            {
                assemblyName = null;
                return false;
            }

            if (ma is string a && !string.IsNullOrWhiteSpace(a))
            {
                assemblyName = a;
                return true;
            }

            throw new ArgumentException("The application domain is invalid.");
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
            Action<HostBuilderContext, DbContextOptionsBuilder> configures)
            where TContext : DbContext
        {
            const string batchBulk = "Microsoft.EntityFrameworkCore.Bulk.RelationalBatchDbContextOptionsExtension`3";
            const string inMemoryBulk = "Microsoft.EntityFrameworkCore.InMemoryBatchExtensions+InMemoryBatchDbContextOptionsExtension";

            return builder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<ModelSupplierService<TContext>>();
                services.AddDbContext<TContext>(
                    optionsLifetime: ServiceLifetime.Singleton,
                    optionsAction: (services, options) =>
                    {
                        var mss = services.GetRequiredService<ModelSupplierService<TContext>>();
                        ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(mss);
                        configures.Invoke(context, options);

                        if (!options.Options.Extensions
                            .Select(e => e.GetType().FullName)
                            .Where(a => a == inMemoryBulk || (a?.StartsWith(batchBulk) ?? false))
                            .Any())
                            throw new InvalidOperationException(NoBulkExtRegistered);
                    });
            });
        }

        /// <summary>
        /// Add a <see cref="DbContext"/> and configure them in the next constructing pipeline.
        /// </summary>
        /// <typeparam name="TContext">The required <see cref="DbContext"/>.</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <param name="configures">The configure delegate</param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder AddDatabase<TContext>(this IHostBuilder builder, Action<DbContextOptionsBuilder> configures) where TContext : DbContext
        {
            return builder.AddDatabase<TContext>((_, b) => configures(b));
        }

        /// <summary>
        /// Shorthand for Configuration.GetSection("ConnectionStrings")[name].
        /// </summary>
        /// <param name="context">The host builder context.</param>
        /// <param name="name">The connection string key.</param>
        /// <returns>The connection string.</returns>
        public static string GetConnectionString(this HostBuilderContext context, string name)
        {
            return context.Configuration.GetConnectionString(name);
        }
    }
}
