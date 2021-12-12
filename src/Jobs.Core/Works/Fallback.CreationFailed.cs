using Jobs.Entities;
using Jobs.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Jobs.Works
{
    public class FallbackCreationFailed : IJobExecutor
    {
        private readonly Exception _reason;

        public FallbackCreationFailed(Exception reason)
        {
            _reason = reason;
        }

        public Task<JobStatus> ExecuteAsync(string arguments, Job entry, ILogger logger)
        {
            logger.LogError(_reason, "Job Executor Creation failed.");
            return Task.FromResult(JobStatus.Failed);
        }
    }
}
