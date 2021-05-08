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
        /// <param name="executor">The job executor.</param>
        /// <returns>Whether the creation is succeeded.</returns>
        public bool TryCreate(string type, out IJobExecutor? executor)
        {
            if (_providers.TryGetValue(type, out var provider))
            {
                executor = provider.Create(_serviceProvider);
                return true;
            }
            else
            {
                executor = null;
                return false;
            }
        }
    }
}
