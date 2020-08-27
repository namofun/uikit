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
}
