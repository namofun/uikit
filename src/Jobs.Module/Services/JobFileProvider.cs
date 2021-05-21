using Jobs.Services;
using Microsoft.Extensions.FileProviders;

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
        /// <param name="path">The job path.</param>
        public PhysicalJobFileProvider(string path) : base(path)
        {
        }
    }
}
