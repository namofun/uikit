using Jobs.Entities;
using Jobs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.JobsModule.Services
{
    public class JobHostedService : BackgroundNotifiableService<JobHostedService>
    {
        private readonly JobExecutorFactory _factory;
        private readonly IJobFileProvider _fileProvider;

        public JobHostedService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _factory = serviceProvider.GetRequiredService<JobExecutorFactory>();
            _fileProvider = serviceProvider.GetRequiredService<IJobFileProvider>();
        }

        protected override async Task ProcessAsync(IServiceScope scope, CancellationToken stoppingToken)
        {
            var scheduler = scope.ServiceProvider.GetRequiredService<IJobScheduler>();
            while (!stoppingToken.IsCancellationRequested)
            {
                var job = await scheduler.DequeueAsync();
                if (job == null) break;

                var executor = _factory.TryCreate(job.JobType, scope.ServiceProvider);
                var logger = new StringBuilderLogger();
                JobStatus result;

                try
                {
                    result = await executor.ExecuteAsync(job.Arguments, job, logger);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unknown exception.");
                    result = JobStatus.Failed;
                }

                await _fileProvider.SaveLogAsync(job, logger.StringBuilder.ToString());
                logger.StringBuilder.Clear();

                await scheduler.MarkAsync(job, result, DateTimeOffset.Now, JobStatus.Running);
            }
        }
    }
}
