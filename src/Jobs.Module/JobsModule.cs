using Jobs.Services;
using Microsoft.AspNetCore.Builder;
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
            services.AddScoped<RelationalJobStorage<TContext>>();
            services.AddScopedUpcast<IJobScheduler, RelationalJobStorage<TContext>>();
            services.AddScopedUpcast<IJobManager, RelationalJobStorage<TContext>>();
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<JobOptions>>().Value.Storage);

            services.PostConfigure<JobOptions>(options =>
            {
                if (options.Storage == null)
                {
                    var path = Path.Combine(environment.ContentRootPath, "JobBlobs");
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    options.Storage ??= new PhysicalJobFileProvider(path);
                }
            });

            services.AddJobExecutor<Jobs.Works.SamplePingPong>();
            services.AddJobExecutor<Jobs.Works.ComposeArchive>();
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Submenu(MenuNameDefaults.DashboardUsers, menu =>
            {
                menu.HasEntry(600)
                    .HasTitle(string.Empty, "Background Jobs")
                    .HasLink("Dashboard", "Jobs", "List");
            });
        }
    }
}
