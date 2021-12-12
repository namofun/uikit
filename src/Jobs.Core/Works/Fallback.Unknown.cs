using Jobs.Entities;
using Jobs.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Jobs.Works
{
    public class FallbackUnknown : IJobExecutor
    {
        public Task<JobStatus> ExecuteAsync(string arguments, Job entry, ILogger logger)
        {
            logger.LogError("Unknown job type.");
            return Task.FromResult(JobStatus.Unknown);
        }
    }
}
