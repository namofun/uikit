using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SatelliteSite.Entities;
using SatelliteSite.IdentityModule.Services;
using System;

namespace SatelliteSite.IdentityModule
{
    public class IdentityModule : AbstractModule
    {
        public const string UserDetail = nameof(UserDetail);

        public const string DashboardUserDetail = nameof(DashboardUserDetail);

        public override string Area => "Account";

        public override void Initialize()
        {
        }

        public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<User, Role>(
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
                .AddEntityFrameworkStores<DefaultContext>()
                .AddUserManager<UserManager>()
                .AddSignInManager<SignInManager>()
                .AddDefaultTokenProviders();

            services.Replace(ServiceDescriptor.Scoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory>());

            services.AddAuthentication()
                .SetCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.LoginPath = "/account/login";
                    options.LogoutPath = "/account/logout";
                    options.AccessDeniedPath = "/account/access-denied";
                    options.SlidingExpiration = true;
                    options.Events = new CookieAuthenticationValidator();
                })
                .AddBasic(options =>
                {
                    options.Realm = "Satellite Site";
                    options.AllowInsecureProtocol = true;
                    options.Events = new BasicAuthenticationValidator();
                });

            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy("EmailVerified", b => b.RequireClaim("email_verified", "true"));
                    options.AddPolicy("HasDashboard", b => b.RequireClaim("dashboard", "true"));
                });

            services.AddSingleton<IEmailSender, SmtpSender>();
            
            services.AddOptions<AuthMessageSenderOptions>()
                .Bind(configuration.GetSection("Mailing"));

            services.AddDbModelSupplier<DefaultContext, IdentityEntityConfiguration>();
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
                    .HasTitle("fas fa-user", "Users")
                    .RequireRoles("Administrator");
            });

            menus.Submenu(MenuNameDefaults.DashboardUsers, menu =>
            {
                menu.HasEntry(0)
                    .HasLink("Dashboard", "Users", "List")
                    .HasTitle(string.Empty, "Users")
                    .RequireRoles("Administrator");
            });

            menus.Component(UserDetail);

            menus.Component(DashboardUserDetail);
        }
    }
}
