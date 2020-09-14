using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Logger definition type.
    /// </summary>
    internal struct AutoMigration { }

    /// <summary>
    /// Provide extension methods to help <see cref="IHost"/> got their <see cref="DbContext"/>s migrated.
    /// </summary>
    public static class AutoMigrationExtensions
    {
        /// <summary>
        /// Automatically migrate the <see cref="DbContext"/> to the newest migration.
        /// </summary>
        /// <typeparam name="TContext">The concrete <see cref="DbContext"/> type</typeparam>
        /// <param name="host">The <see cref="IHost"/></param>
        /// <returns>The <see cref="IHost"/></returns>
        public static IHost AutoMigrate<TContext>(this IHost host) where TContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<TContext>();
                if (context.Database.GetPendingMigrations().Any())
                    context.Database.Migrate();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<AutoMigration>>();
                logger.LogError(ex, "An error occurred during migrating the database.");
                throw;
            }

            return host;
        }
    }
}
