using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        /// The default memory cache.
        /// </summary>
        public IMemoryCache Cache { get; }

        /// <summary>
        /// The default set.
        /// </summary>
        DbSet<Configuration> Configurations => Context.Set<Configuration>();

        /// <summary>
        /// Constructs a registry.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="cache">The memory cache.</param>
        public ConfigurationRegistry(TContext context, ConfigurationRegistryCache cache)
        {
            Context = context;
            Cache = cache;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(string name, string newValue)
        {
            var result = await Configurations
                .Where(c => c.Name == name)
                .BatchUpdateAsync(c => new Configuration { Value = newValue });

            Cache.Remove(name);
            return result == 1;
        }

        /// <inheritdoc />
        public Task<Configuration?> FindAsync(string config)
        {
            return Cache.GetOrCreateAsync(config,
            async entry =>
            {
                var config = (string)entry.Key;
                var result = await Configurations.FindAsync(config);
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return result;
            })!;
        }

        /// <inheritdoc />
        public async Task<List<Configuration>> GetAsync(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return await Configurations.ToListAsync();
            }

            var item = await FindAsync(name);
            if (item == null)
            {
                return new List<Configuration>(0);
            }
            else
            {
                return new List<Configuration>(1) { item };
            }
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
            var conf = await FindAsync(name);

            if (conf == null)
            {
                throw new KeyNotFoundException(
                    $"The configuration {name} is not saved. Please check your migration status.");
            }
            else if (conf.Type != typeName)
            {
                throw new InvalidCastException(
                    $"The type of configuration {name} is not correct.");
            }

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
