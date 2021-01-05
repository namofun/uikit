using Microsoft.EntityFrameworkCore;
using SatelliteSite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.Services
{
    /// <summary>
    /// Entity Framework Core implementation for <see cref="IConfigurationRegistry"/>.
    /// </summary>
    /// <typeparam name="TContext">The context type.</typeparam>
    public class ConfigurationRegistry<TContext> : IConfigurationRegistry
        where TContext : DbContext
    {
        /// <summary>
        /// The default context.
        /// </summary>
        public TContext Context { get; }

        /// <summary>
        /// The default set.
        /// </summary>
        DbSet<Configuration> Configurations => Context.Set<Configuration>();

        /// <summary>
        /// Constructs a registry.
        /// </summary>
        /// <param name="context">The database context.</param>
        public ConfigurationRegistry(TContext context)
        {
            Context = context;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(string name, string newValue)
        {
            var result = await Configurations
                .Where(c => c.Name == name)
                .BatchUpdateAsync(c => new Configuration { Value = newValue });
            return result == 1;
        }

        /// <inheritdoc />
        public Task<Configuration?> FindAsync(string config)
        {
            return Configurations
                .Where(c => c.Name == config)
                .SingleOrDefaultAsync()!;
        }

        /// <inheritdoc />
        public Task<List<Configuration>> GetAsync(string? name)
        {
            return Configurations
                .WhereIf(name != null, c => c.Name == name)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<ILookup<string, Configuration>> ListAsync()
        {
            var conf = await Configurations
                .Where(c => c.Public)
                .ToListAsync();
            return conf.OrderBy(c => c.DisplayPriority)
                .ToLookup(c => c.Category);
        }

        /// <summary>
        /// Gets the typed configuration with corresponding name.
        /// </summary>
        /// <param name="name">The configuration name.</param>
        /// <param name="typeName">The configuration type name.</param>
        /// <returns>The task for configuration result or <c>null</c>.</returns>
        private async Task<T> GetValueAsync<T>(string name, string typeName)
        {
            var conf = await Configurations
                .Where(c => c.Name == name && c.Type == typeName)
                .Select(c => new { c.Value })
                .SingleOrDefaultAsync();

            if (conf == null)
                throw new KeyNotFoundException($"The configuration {name} is not saved. Please check your migration status.");

            return conf.Value.AsJson<T>();
        }

        /// <inheritdoc />
        public Task<bool?> GetBooleanAsync(string name)
        {
            return GetValueAsync<bool?>(name, "bool");
        }

        /// <inheritdoc />
        public Task<int?> GetIntegerAsync(string name)
        {
            return GetValueAsync<int?>(name, "int");
        }

        /// <inheritdoc />
        public Task<DateTimeOffset?> GetDateTimeOffsetAsync(string name)
        {
            return GetValueAsync<DateTimeOffset?>(name, "datetime");
        }

        /// <inheritdoc />
        public Task<string?> GetStringAsync(string name)
        {
            return GetValueAsync<string?>(name, "string");
        }
    }
}
