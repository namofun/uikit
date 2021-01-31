using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SatelliteSite.Services;

namespace SatelliteSite.Substrate
{
    internal class DefaultModule<TContext> : AbstractModule
        where TContext : DbContext
    {
        public override string Area => string.Empty;

        public override void Initialize()
        {
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Menu(MenuNameDefaults.MainNavbar, menu =>
            {

            });

            menus.Menu(MenuNameDefaults.DashboardNavbar, admin =>
            {

            });

            menus.Menu(MenuNameDefaults.DashboardContent, menu =>
            {
                menu.HasSubmenu(MenuNameDefaults.DashboardConfigurations, 0, conf =>
                {
                    conf.HasTitle(string.Empty, "Infrastructure")
                        .HasLink("javascript:;");

                    conf.HasEntry(0)
                        .HasLink("Dashboard", "Root", "Config")
                        .HasTitle(string.Empty, "Configuration settings")
                        .RequireRoles("Administrator");
                });

                menu.HasSubmenu(MenuNameDefaults.DashboardUsers, 100, user =>
                {
                    user.HasTitle(string.Empty, "Users")
                        .HasLink("javascript:;");
                });

                menu.HasSubmenu(MenuNameDefaults.DashboardDocuments, 200, docs =>
                {
                    docs.HasTitle(string.Empty, "Documentation")
                        .HasLink("javascript:;");

                    docs.HasEntry(100)
                        .HasTitle(string.Empty, "Sitemaps")
                        .HasLink("Dashboard", "Root", "Endpoints");

                    docs.HasEntry(101)
                        .HasTitle(string.Empty, "Component Versions")
                        .HasLink("Dashboard", "Root", "Versions");

                    docs.HasEntry(102)
                        .HasTitle(string.Empty, "Auditlogs")
                        .HasLink("Dashboard", "Root", "Auditlog")
                        .RequireRoles("Administrator");
                });
            });
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.TryAddSingleton<ITelemetryClient, NullTelemetryClient>();
            services.AddScoped<IAuditlogger, Auditlogger<TContext>>();
            services.AddSingleton<ConfigurationRegistryCache>();
            services.AddScoped<IConfigurationRegistry, ConfigurationRegistry<TContext>>();
            services.AddDbModelSupplier<TContext, CoreEntityConfiguration<TContext>>();
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();
            endpoints.MapFallNotFound("/api/{**slug}");
            endpoints.MapFallNotFound("/lib/{**slug}");
            endpoints.MapFallNotFound("/images/{**slug}");
        }
    }
}
