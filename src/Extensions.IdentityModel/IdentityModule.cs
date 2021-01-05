using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using SatelliteSite;
using SatelliteSite.Entities;
using SatelliteSite.IdentityModule.Services;
using System;

[assembly: RoleDefinition(1, "Administrator", "admin", "Administrative User")]
[assembly: RoleDefinition(2, "Blocked", "blocked", "Blocked User")]
[assembly: ConfigurationBoolean(0, "Identity", "enable_register", true, "Whether to allow user self registration.")]

namespace SatelliteSite.IdentityModule
{
    public class IdentityModule<TUser, TRole, TContext> : AbstractModule, IAuthorizationPolicyRegistry
        where TUser : Entities.User, new()
        where TRole : Entities.Role, new()
        where TContext : IdentityDbContext<TUser, TRole, int>
    {
        public override string Area => "Account";

        public override bool ProvideIdentity => true;

        public override void Initialize()
        {
        }

        public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
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
                .AddEntityFrameworkStores<TContext>()
                .AddUserManager<UserManager<TUser, TRole>>()
                .AddSignInManager<SignInManager2<TUser>>()
                .AddDefaultTokenProviders();

            services.ReplaceScoped<
                IUserClaimsPrincipalFactory<TUser>,
                UserClaimsPrincipalFactory<TUser, TRole, TContext>>();

            var memoryCacheOptions = new MemoryCacheOptions() { Clock = new SystemClock() };
            var basicCache = new MemoryCache(memoryCacheOptions);
            var cookieCache = new SlideExpirationService(new MemoryCache(memoryCacheOptions));

            services.AddSingleton(cookieCache);

            services.ConfigureApplicationCookie(
                options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.LoginPath = "/account/login";
                    options.LogoutPath = "/account/logout";
                    options.AccessDeniedPath = "/account/access-denied";
                    options.SlidingExpiration = true;
                    options.Events = new CookieAuthenticationValidator(cookieCache);
                });

            services.AddAuthentication()
                .AddBasic(options =>
                {
                    options.Realm = "Satellite Site";
                    options.AllowInsecureProtocol = true;
                    options.Events = new BasicAuthenticationValidator(basicCache);
                });

            services.AddAuthorization();
            services.AddSingleton<IConfigureOptions<AuthorizationOptions>, ConfigureAuthoraztionPolicy>();

            services.AddScoped<IUserManager>(s => s.GetRequiredService<UserManager<TUser, TRole>>());
            services.AddScoped<ISignInManager>(s => s.GetRequiredService<SignInManager2<TUser>>());

            services.AddSingleton<IEmailSender, SmtpSender>();
            
            services.AddOptions<AuthMessageSenderOptions>()
                .Bind(configuration.GetSection("Mailing"));

            services.AddDbModelSupplier<TContext, IdentityEntityConfiguration<TUser, TRole, TContext>>();
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
