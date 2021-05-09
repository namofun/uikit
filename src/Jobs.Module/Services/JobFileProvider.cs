using Jobs.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace SatelliteSite.JobsModule.Services
{
    /// <summary>
    /// The file provider interface for jobs.
    /// </summary>
    public class PhysicalJobFileProvider : PhysicalMutableFileProvider, IJobFileProvider
    {
        /// <summary>
        /// Initialize the job file provider.
        /// </summary>
        /// <param name="options">The job options.</param>
        public PhysicalJobFileProvider(IOptions<JobOptions> options) : base(options.Value.Directory)
        {
        }
    }
}
