using System;
using System.Collections.Generic;
using System.Linq;

namespace Jobs.Services
{
    /// <summary>
    /// The factory to create <see cref="IJobExecutor"/>.
    /// </summary>
    public sealed class JobExecutorFactory
    {
        private readonly IReadOnlyDictionary<string, IJobExecutorProvider> _providers;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initialize the job executor factory.
        /// </summary>
        /// <param name="providers">The job executor providers.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public JobExecutorFactory(
            IEnumerable<IJobExecutorProvider> providers,
            IServiceProvider serviceProvider)
        {
            _providers = providers.ToDictionary(k => k.Type);
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Tries to create an instance of <see cref="IJobExecutor"/>.
        /// </summary>
        /// <param name="type">The job type.</param>
        /// <returns>The job executor.</returns>
        public IJobExecutor TryCreate(string type)
        {
            if (_providers.TryGetValue(type, out var provider))
            {
                try
                {
                    return provider.Create(_serviceProvider);
                }
                catch (Exception ex)
                {
                    return new Works.FallbackCreationFailed(ex);
                }
            }
            else
            {
                return new Works.FallbackUnknown();
            }
        }
    }
}
