using Microsoft.EntityFrameworkCore;
using SatelliteSite.Entities;
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
    }
}
