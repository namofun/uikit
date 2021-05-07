using Microsoft.Extensions.Logging;
using SatelliteSite.Jobs.Entities;
using System;
using System.Threading.Tasks;

namespace SatelliteSite.Jobs.Services
{
    /// <summary>
    /// The executor of job.
    /// </summary>
    public interface IJobExecutor
    {
        /// <summary>
        /// Asynchronously execute the targeted job with argument.
        /// Note that this function shouldn't throw any exceptions.
        /// </summary>
        /// <param name="arguments">The job arguments.</param>
        /// <param name="guid">The storage unique ID.</param>
        /// <param name="logger">The job logger.</param>
        /// <returns>The job status.</returns>
        Task<JobStatus> ExecuteAsync(string arguments, Guid guid, ILogger logger);
    }
}
