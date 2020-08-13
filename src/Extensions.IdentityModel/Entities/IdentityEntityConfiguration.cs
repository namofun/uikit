using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq;
using System.Security.Claims;

namespace SatelliteSite.Entities
{
    public class IdentityEntityConfiguration :
        EntityTypeConfigurationSupplier<DefaultContext>,
        IEntityTypeConfiguration<User>,
        IEntityTypeConfiguration<Role>,
        IEntityTypeConfiguration<IdentityRoleClaim<int>>,
        IEntityTypeConfiguration<IdentityUserRole<int>>
    {
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

        public void Configure(EntityTypeBuilder<User> entity)
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
        }

        public void Configure(EntityTypeBuilder<IdentityRoleClaim<int>> entity)
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
        }

        public void Configure(EntityTypeBuilder<Role> entity)
        {
            entity.HasData(HasRoles);
        }

        public void Configure(EntityTypeBuilder<IdentityUserRole<int>> entity)
        {
            entity.HasData(
                new IdentityUserRole<int> { RoleId = OfRole("Judgehost"), UserId = -1 });
        }
    }
}
