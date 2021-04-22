using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.Services;
using System;

namespace SatelliteSite.Substrate
{
    internal class DefaultModule : AbstractModule
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
                });
            });

            // If someone reads this code then he'll know how to customize it.
            menus.Submenu(MenuNameDefaults.UserDropdown, menu =>
            {
                menu.HasEntry(0)
                    .HasTitle("fas fa-address-card", "Profile")
                    .HasLink((u, c) => u.Action("Show", "Profile", new { area = "Account", username = c.HttpContext.User.GetUserName() }));

                menu.HasEntry(100)
                    .HasTitle("fas fa-compass", "Dashboard")
                    .HasLink("Dashboard", "Root", "Index")
                    .WithMetadata("ExcludeMenuNameAt", MenuNameDefaults.DashboardNavbar)
                    .WithMetadata("RequiredPolicy", "HasDashboard");

                menu.HasEntry(200)
                    .HasTitle("fas fa-home", "Back to main site")
                    .HasLink("/")
                    .WithMetadata("ExcludeMenuNameAt", MenuNameDefaults.MainNavbar);
            });
        }

        protected virtual void RegisterSubstrateBase(IServiceCollection services)
        {
            services.AddScoped<IAuditlogger, NullAuditlogger>();
            services.AddScoped<IConfigurationRegistry, NullConfigurationRegistry>();
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ITelemetryClient, NullTelemetryClient>();
            services.AddScoped<IUserInformationProvider, NullUserInformationProvider>();
            RegisterSubstrateBase(services);
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();
            endpoints.MapFallNotFound("/api/{**slug}");
            endpoints.MapFallNotFound("/lib/{**slug}");
            endpoints.MapFallNotFound("/images/{**slug}");

            endpoints.WithErrorHandler("Dashboard", "Root")
                .MapStatusCode("/dashboard/{**slug}");
        }
    }

    internal class DefaultModule<TContext> : DefaultModule where TContext : DbContext
    {
        public override void RegisterMenu(IMenuContributor menus)
        {
            base.RegisterMenu(menus);

            menus.Submenu(MenuNameDefaults.DashboardConfigurations, menu =>
            {
                menu.HasEntry(-100)
                    .HasLink("Dashboard", "Root", "Config")
                    .HasTitle(string.Empty, "Configuration settings")
                    .RequireRoles("Administrator");
            });

            menus.Submenu(MenuNameDefaults.DashboardDocuments, menu =>
            {
                menu.HasEntry(102)
                    .HasTitle(string.Empty, "Auditlogs")
                    .HasLink("Dashboard", "Root", "Auditlog")
                    .RequireRoles("Administrator");
            });
        }

        protected override void RegisterSubstrateBase(IServiceCollection services)
        {
            services.AddScoped<IAuditlogger, Auditlogger<TContext>>();
            services.AddSingleton<ConfigurationRegistryCache>();
            services.AddScoped<IConfigurationRegistry, ConfigurationRegistry<TContext>>();
            services.AddDbModelSupplier<TContext, CoreEntityConfiguration<TContext>>();
        }
    }
}
