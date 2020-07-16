﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SatelliteSite.Entities;
using SatelliteSite.IdentityModule.Services;
using System;
using System.Linq;
using System.Security.Claims;

namespace SatelliteSite.IdentityModule
{
    public class IdentityModule : AbstractModule<DefaultContext>
    {
        public override string Area => "Account";

        public override void Initialize()
        {
        }

        public override void RegisterServices(IServiceCollection services)
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
                });
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
        }

        public static readonly Role[] HasRoles = new[]
        {
            new Role { Id = -1, ConcurrencyStamp = "17337d8e-0118-4c42-9da6-e4ca600d5836", Name = "Administrator", NormalizedName = "ADMINISTRATOR", ShortName = "admin", Description = "Administrative User" },
            new Role { Id = -2, ConcurrencyStamp = "9ec57e90-312c-4eed-ac25-a40fbcf5f33b", Name = "Blocked", NormalizedName = "BLOCKED", ShortName = "blocked", Description = "Blocked User" },
            new Role { Id = -3, ConcurrencyStamp = "2bbf420d-6253-4ace-a825-4bf8e85cf41e", Name = "Problem", NormalizedName = "PROBLEM", ShortName = "prob", Description = "Problem Provider" },
            new Role { Id = -4, ConcurrencyStamp = "fd0d1cf4-2ccf-4fd6-9d47-7fd62923c5d2", Name = "Judgehost", NormalizedName = "JUDGEHOST", ShortName = "judgehost", Description = "(Internal/System) Judgehost" },
            new Role { Id = -5, ConcurrencyStamp = "81ffd1be-883c-4093-8adf-f2a4909370b7", Name = "CDS", NormalizedName = "CDS", ShortName = "cds_api", Description = "CDS API user" },
        };

        public static int OfRole(string role) => HasRoles.Single(r => r.Name == role).Id;

        public static int[] OfRoles(params string[] roles) => roles.Select(r => OfRole(r)).ToArray();

        public static readonly (Claim, int[])[] RoleClaims = new[]
        {
            (new Claim("dashboard", "true"), OfRoles("Administrator", "Problem")),
            (new Claim("create_contest", "true"), OfRoles("Administrator")),
            (new Claim("create_problem", "true"), OfRoles("Administrator", "Problem")),
            (new Claim("plag_detect", "true"), OfRoles("Administrator")),
            (new Claim("judger", "true"), OfRoles("Judgehost")),
            (new Claim("read_contest", "true"), OfRoles("Administrator", "CDS")),
        };

        public override void RegisterEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasData(HasRoles);
            });

            modelBuilder.Entity<IdentityRoleClaim<int>>(entity =>
            {
                entity.HasData(RoleClaims
                    .SelectMany(
                        collectionSelector: c => c.Item2,
                        resultSelector: (c, i) => new { claim = c.Item1, roleId = i })
                    .Select(
                        selector: (a, i) => new IdentityRoleClaim<int>
                        {
                            Id = -1 - i,
                            RoleId = a.roleId,
                            ClaimType = a.claim.Type,
                            ClaimValue = a.claim.Value
                        }));
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasData(
                    new User
                    {
                        Id = -1,
                        UserName = "judgehost",
                        NormalizedUserName = "JUDGEHOST",
                        Email = "acm@xylab.fun",
                        NormalizedEmail = "ACM@XYLAB.FUN",
                        EmailConfirmed = true,
                        ConcurrencyStamp = "e1a1189a-38f5-487b-907b-6d0533722f02",
                        SecurityStamp = "AAAABBBBCCCCDDDDEEEEFFFFGGGGHHHH",
                        LockoutEnabled = false,
                        NickName = "User for judgedaemons"
                    });

                entity.Property(u => u.NickName)
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<IdentityUserRole<int>>(entity =>
            {
                entity.HasData(
                    new IdentityUserRole<int> { RoleId = OfRole("Judgehost"), UserId = -1 });
            });

            modelBuilder.Entity<IdentityRoleClaim<int>>(entity =>
            {
                entity.HasData(RoleClaims
                    .SelectMany(
                        collectionSelector: c => c.Item2,
                        resultSelector: (c, i) => new { claim = c.Item1, roleId = i })
                    .Select(
                        selector: (a, i) => new IdentityRoleClaim<int>
                        {
                            Id = -1 - i,
                            RoleId = a.roleId,
                            ClaimType = a.claim.Type,
                            ClaimValue = a.claim.Value
                        }));
            });
        }
    }
}