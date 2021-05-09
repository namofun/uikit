using Jobs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.JobsModule.Services;

namespace SatelliteSite.JobsModule
{
    public class JobsModule<TUser, TContext> : AbstractModule
        where TUser : IdentityModule.Entities.User
        where TContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public override string Area => "Jobs";

        public override void Initialize()
        {
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<JobExecutorFactory>();
            services.AddSingleton<IJobFileProvider, PhysicalJobFileProvider>();
            services.AddHostedService<JobHostedService>();
        }
    }
}
