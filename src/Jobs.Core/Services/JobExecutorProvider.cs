using Microsoft.Extensions.DependencyInjection;
using System;

namespace Jobs.Services
{
    /// <summary>
    /// The conventional interface for creating an <see cref="IJobExecutor"/>.
    /// Note that this interface will be registered as <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    public interface IJobExecutorProvider
    {
        /// <summary>
        /// The job type
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Creates the <see cref="IJobExecutor"/>.
        /// </summary>
        /// <param name="serviceProvider">The job service provider.</param>
        /// <returns>The created <see cref="IJobExecutor"/>.</returns>
        IJobExecutor Create(IServiceProvider serviceProvider);
    }
}
