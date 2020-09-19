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
    public static class DataAccessExtensions
    {
        /// <summary>
        /// The name of migration assembly
        /// </summary>
        public static string? MigrationAssembly { get; set; }

        /// <summary>
        /// Mark the migration assembly.
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
            return builder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<TContext>(options =>
                {
                    options.ReplaceService<IModelCustomizer, SuppliedModelCustomizer<TContext>>();
                    configures.Invoke(context.Configuration, options);
                });
            });
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
    }
}
