using Jobs.Entities;
using Jobs.Services;
using Microsoft.Extensions.DependencyInjection;
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

                var executor = _factory.TryCreate(job.JobType);
                var logger = new StringBuilderLogger();
                var result = await executor.ExecuteAsync(job.Arguments, job.JobId, logger);
                await _fileProvider.WriteStringAsync(job.JobId + "/log", logger.StringBuilder.ToString());
                logger.StringBuilder.Clear();

                await scheduler.MarkAsync(job, result, DateTimeOffset.Now, JobStatus.Running);
            }
        }
    }
}
