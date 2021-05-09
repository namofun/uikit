using Jobs.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// The extensions on dependency injection for jobs.
    /// </summary>
    public static class JobExtensions
    {
        /// <summary>
        /// Registers the job executor provider into dependency injection collection.
        /// </summary>
        /// <typeparam name="TProvider">The provider type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddJobExecutor<TProvider>(this IServiceCollection services) where TProvider : class, IJobExecutorProvider
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IJobExecutorProvider, TProvider>());
            return services;
        }
    }
}
