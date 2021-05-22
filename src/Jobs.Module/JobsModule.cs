﻿using Jobs.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SatelliteSite.JobsModule.Services;
using System.IO;

namespace SatelliteSite.JobsModule
{
    public class JobsModule<TUser, TContext> : AbstractModule
        where TUser : IdentityModule.Entities.User, new()
        where TContext : DbContext
    {
        public override string Area => "Jobs";

        public override void Initialize()
        {
        }

        public override void RegisterServices(
            IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            services.AddSingleton<JobExecutorFactory>();
            services.AddHostedService<JobHostedService>();
            services.AddDbModelSupplier<TContext, JobsEntityConfiguration<TUser, TContext>>();
            services.AddScoped<IJobScheduler, RelationalJobScheduler<TContext>>();
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<JobOptions>>().Value.Storage);

            services.PostConfigure<JobOptions>(options =>
            {
                options.Storage ??= new PhysicalJobFileProvider(
                    Path.Combine(environment.ContentRootPath, "JobBlobs"));
            });
        }
    }
}
