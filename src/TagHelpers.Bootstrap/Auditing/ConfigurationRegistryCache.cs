using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace SatelliteSite.Services
{
    /// <summary>
    /// The cache provider for <see cref="IConfigurationRegistry"/>.
    /// </summary>
    /// <remarks>This service will be registered as <see cref="ServiceLifetime.Singleton"/>.</remarks>
    public class ConfigurationRegistryCache : IMemoryCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<MemoryCacheOptions> _options;

        /// <inheritdoc />
        public ICacheEntry CreateEntry(object key)
        {
            return _memoryCache.CreateEntry(key);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _memoryCache.Dispose();
        }

        /// <inheritdoc />
        public void Remove(object key)
        {
            _memoryCache.Remove(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(object key, out object value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }

        /// <summary>
        /// Instantiate a registry cache.
        /// </summary>
        public ConfigurationRegistryCache()
        {
            _options = Options.Create(new MemoryCacheOptions
            {
                Clock = new SystemClock(),
            });

            _memoryCache = new MemoryCache(_options);
        }
    }
}
