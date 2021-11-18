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

        /// <summary>
        /// Initialize the job executor factory.
        /// </summary>
        /// <param name="providers">The job executor providers.</param>
        public JobExecutorFactory(IEnumerable<IJobExecutorProvider> providers)
        {
            _providers = providers.ToDictionary(k => k.Type);
        }

        /// <summary>
        /// Tries to create an instance of <see cref="IJobExecutor"/>.
        /// </summary>
        /// <param name="type">The job type.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The job executor.</returns>
        public IJobExecutor TryCreate(string type, IServiceProvider serviceProvider)
        {
            if (_providers.TryGetValue(type, out var provider))
            {
                try
                {
                    return provider.Create(serviceProvider);
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
