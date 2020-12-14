using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Data access extensions for Substrate.
    /// </summary>
    public static class HostBuilderDataAccessExtensions
    {
        /// <summary>
        /// Mark the migration assembly.
        /// </summary>
        /// <typeparam name="T">The program class.</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder MarkDomain<T>(this IHostBuilder builder)
        {
            builder.Properties["MigrationAssembly"]
                =  typeof(T).Assembly.GetName().Name
                ?? throw new ArgumentNullException("The migration assembly is invalid.");
            return builder;
        }

        /// <summary>
        /// Disable the MigrationAssembly because we are in tests.
        /// </summary>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder MarkTest(this IHostBuilder builder)
        {
            builder.Properties["ShouldNotUseMigrationAssembly"] = true;
            return builder;
        }

        /// <summary>
        /// Get the migration assembly information.
        /// </summary>
        /// <param name="builder">The host builder.</param>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>Whether migration assembly is used.</returns>
        internal static bool MigrationAssembly(this IHostBuilder builder, out string? assemblyName)
        {
            if (builder.Properties.ContainsKey("ShouldNotUseMigrationAssembly"))
            {
                assemblyName = null;
                return false;
            }

            if (!builder.Properties.TryGetValue("MigrationAssembly", out var ma) || !(ma is string maa))
                throw new ArgumentException("The migration assembly is invalid.");

            assemblyName = maa;
            return true;
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
            Action<IConfiguration, DbContextOptionsBuilder> configures)
            where TContext : DbContext
        {
            return builder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<ModelSupplierService<TContext>>();
                services.AddDbContext<TContext>(
                    optionsLifetime: ServiceLifetime.Singleton,
                    optionsAction: (services, options) =>
                    {
                        var mss = services.GetRequiredService<ModelSupplierService<TContext>>();
                        ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(mss);
                        configures.Invoke(context.Configuration, options);
                    });
            });
        }

        /// <summary>
        /// Add a <see cref="DbContext"/> and configure them in the next constructing pipeline.
        /// </summary>
        /// <typeparam name="TContext">The required <see cref="DbContext"/>.</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/>.</param>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <param name="configures1">The dbcontext options builder.</param>
        /// <param name="configures2">The provider options builder.</param>
        /// <returns>The <see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder AddDatabaseMssql<TContext>(
            this IHostBuilder builder,
            string connectionStringName,
            Action<DbContextOptionsBuilder>? configures1 = null,
            Action<SqlServerDbContextOptionsBuilder>? configures2 = null)
            where TContext : DbContext
        {
            if (!builder.Properties.ContainsKey("ShouldNotUseMigrationAssembly") &&
                !builder.Properties.ContainsKey("MigrationAssembly"))
                throw new ArgumentException("The migration assembly is invalid.");

            return builder.AddDatabase<TContext>((conf, opt) =>
            {
                opt.UseSqlServer(
                    conf.GetConnectionString(connectionStringName),
                    sqlServerOptionsAction: o =>
                    {
                        o.UseBulk();
                        if (!builder.Properties.ContainsKey("ShouldNotUseMigrationAssembly"))
                            o.MigrationsAssembly((string)builder.Properties["MigrationAssembly"]);
                        configures2?.Invoke(o);
                    });

                configures1?.Invoke(opt);
            });
        }

        /// <summary>
        /// Add a <see cref="DbContext"/> and configure them in the next constructing pipeline.
        /// </summary>
        /// <typeparam name="TContext">The required <see cref="DbContext"/>.</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/>.</param>
        /// <param name="databaseName">The in-memory database name.</param>
        /// <param name="configures1">The dbcontext options builder.</param>
        /// <param name="configures2">The provider options builder.</param>
        /// <returns>The <see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder AddDatabaseInMemory<TContext>(
            this IHostBuilder builder,
            string databaseName,
            Action<DbContextOptionsBuilder>? configures1 = null,
            Action<InMemoryDbContextOptionsBuilder>? configures2 = null)
            where TContext : DbContext
        {
            return builder.AddDatabase<TContext>((conf, opt) =>
            {
                opt.UseInMemoryDatabase(
                    databaseName,
                    inMemoryOptionsAction: o =>
                    {
                        o.UseBulk();
                        configures2?.Invoke(o);
                    });

                configures1?.Invoke(opt);
            });
        }
    }
}
