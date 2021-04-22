using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SatelliteSite;
using SatelliteSite.Services;
using System;
using System.Linq;

[assembly: RoleDefinition(1, "Administrator", "admin", "Administrative User")]
[assembly: RoleDefinition(2, "Blocked", "blocked", "Blocked User")]
[assembly: ConfigurationBoolean(0, "Identity", "enable_register", true, "Whether to allow user self registration.")]

namespace SatelliteSite.IdentityModule
{
    public class IdentityModule<TUser, TRole, TContext> : AbstractModule, IAuthorizationPolicyRegistry
        where TUser : Entities.User, new()
        where TRole : Entities.Role, new()
        where TContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<TUser, TRole, int>
    {
        public override string Area => "Account";

        public override bool ProvideIdentity => true;

        public override void Initialize()
        {
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddIdentity<TUser, TRole>(
                options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredUniqueChars = 2;

                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 10;
                    options.Lockout.AllowedForNewUsers = true;

                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.@";

                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                })
                .AddUserStore<UserStore<TUser, TRole, TContext>>()
                .AddRoleStore<RoleStore<TRole, TContext>>()
                .AddUserManager<UserManager<TUser, TRole>>()
                .AddSignInManager<CompatibleSignInManager<TUser>>()
                .AddDefaultTokenProviders();

            services.ReplaceScoped<
                IUserClaimsPrincipalFactory<TUser>,
                FullUserClaimsPrincipalFactory<TUser, TRole>>();

            services.AddScoped<
                ILightweightUserClaimsPrincipalFactory<TUser>,
                LightweightUserClaimsPrincipalFactory<TUser, TRole>>();

            services.AddScoped<CompositeUserClaimsProvider>();

            services.AddSingleton<BasicAuthenticationValidator>();
            services.AddSingleton<CookieAuthenticationValidator>();
            services.AddSingleton<ISignInSlideExpiration, DefaultSignInSlideExpiration<TUser>>();
            services.TryAddSingleton(typeof(IUserInformationCache<>), typeof(MemoryUserInformationCache<>));

            services.AddAuthentication().AddBasic();
            services.AddAuthorization();
            services.ConfigureOptions<ConfigureAuthoraztionPolicy>();
            services.ConfigureOptions<SubstrateSiteNameConfigurator>();
            services.ConfigureOptions<IdentityAdvancedConfigurator>();
            services.ConfigureOptions<AuthenticateSchemeConfigurator>();

            services.AddScopedUpcast<IUserManager, UserManager<TUser, TRole>>();
            services.AddScopedUpcast<ISignInManager, CompatibleSignInManager<TUser>>();

            services.AddSingleton<IEmailSender, SmtpSender>();
            services.AddOptions<AuthMessageSenderOptions>();

            services.AddDbModelSupplier<TContext, IdentityEntityConfiguration<TUser, TRole, TContext>>();

            if (services
                .Where(d => d.ServiceType == typeof(IUserInformationProvider) && d.Lifetime == ServiceLifetime.Scoped)
                .Single().ImplementationType == typeof(NullUserInformationProvider))
            {
                services.ReplaceScoped<IUserInformationProvider, DefaultUserInformationProvider>();
            }
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Menu(MenuNameDefaults.DashboardNavbar, menu =>
            {
                menu.HasEntry(500)
                    .HasLink("Dashboard", "Users", "List")
                    .HasTitle("fas fa-address-card", "Users")
                    .ActiveWhenController("Users")
                    .RequireRoles("Administrator");
            });

            menus.Submenu(MenuNameDefaults.DashboardUsers, menu =>
            {
                menu.HasEntry(0)
                    .HasLink("Dashboard", "Users", "List")
                    .HasTitle(string.Empty, "Users")
                    .RequireRoles("Administrator");
            });

            menus.Menu(ExtensionPointDefaults.UserDetailMenu, menu =>
            {
                menu.HasEntry(0)
                    .HasLink("Account", "Profile", "Edit")
                    .HasTitle("primary", "Edit profile")
                    .RequireThat(c => c.HttpContext.User.GetUserName() == ((IUser)c.ViewData["User"]).UserName);

                menu.HasEntry(10)
                    .HasLink("Account", "Profile", "ChangePassword")
                    .HasTitle("secondary", "Change password")
                    .RequireThat(c => c.HttpContext.User.GetUserName() == ((IUser)c.ViewData["User"]).UserName);

                menu.HasEntry(0)
                    .HasLink((urlHelper, context) => urlHelper.Action("Detail", "Users", new { area = "Dashboard", uid = ((IUser)context.ViewData["User"]).Id }))
                    .HasTitle("danger", "Dashboard")
                    .RequireThat(c => c.HttpContext.User.GetUserName() != ((IUser)c.ViewData["User"]).UserName)
                    .RequireRoles("Administrator");
            });

            menus.Component(ExtensionPointDefaults.UserDetail);

            menus.Component(ExtensionPointDefaults.DashboardUserDetail);
        }

        public void RegisterPolicies(IAuthorizationPolicyContainer container)
        {
            container.AddPolicy("EmailVerified", b => b.RequireClaim("email_verified", "true"));
            container.AddPolicy2("HasDashboard", b => b.AcceptRole("Administrator"));
        }
    }
}
