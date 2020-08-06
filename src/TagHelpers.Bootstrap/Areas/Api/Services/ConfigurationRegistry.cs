using Microsoft.EntityFrameworkCore;
using SatelliteSite.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.Services
{
    /// <summary>
    /// The registry of dynamic configurations.
    /// </summary>
    public interface IConfigurationRegistry
    {
        /// <summary>
        /// Gets the list of public configurations.
        /// </summary>
        /// <returns>The task for configuration lookup, grouped by category.</returns>
        Task<ILookup<string, Configuration>> ListAsync();

        /// <summary>
        /// Gets the configuration with corresponding name.
        /// </summary>
        /// <param name="config">The configuration name.</param>
        /// <returns>The task for configuration entity.</returns>
        Task<Configuration?> FindAsync(string config);

        /// <summary>
        /// Sets the configuration with corresponding value.
        /// </summary>
        /// <param name="name">The name of configuration.</param>
        /// <param name="newValue">The value of configuration.</param>
        /// <returns>The task for updating entities success.</returns>
        Task<bool> UpdateAsync(string name, string newValue);

        /// <summary>
        /// Gets the list of configurations.
        /// </summary>
        /// <param name="name">The name of configuration. If null, then the whole list.</param>
        /// <returns>The configuration list.</returns>
        Task<List<Configuration>> GetAsync(string? name = null);
    }

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
    }
}
