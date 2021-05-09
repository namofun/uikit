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

        public JobHostedService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _factory = serviceProvider.GetRequiredService<JobExecutorFactory>();
        }

        protected override Task ProcessAsync(IServiceScope scope, CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
