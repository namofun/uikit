using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SatelliteSite.Entities;
using System.Linq;
using Md = SatelliteSite.IdentityModule.IdentityModule;

namespace SatelliteSite
{
    public class DefaultContext : IdentityDbContext<User, Role, int>
    {
        public DbSet<Configuration> Configurations { get; set; }

        public DefaultContext(DbContextOptions<DefaultContext> options)
            : base(options)
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Configuration>(entity =>
            {
                entity.HasKey(e => e.Name);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .IsUnicode(false)
                    .HasMaxLength(128);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Category)
                    .IsRequired();

                entity.Property(e => e.Type)
                    .IsRequired();

                entity.HasData(
                    new Configuration { DisplayPriority = 1, Category = "Judging", Name = "process_limit", Value = "64", Type = "int", Description = "Maximum number of processes that the submission is allowed to start (including shell and possibly interpreters).", Public = true },
                    new Configuration { DisplayPriority = 2, Category = "Judging", Name = "script_timelimit", Value = "30", Type = "int", Description = "Maximum seconds available for compile/compare scripts. This is a safeguard against malicious code and buggy scripts, so a reasonable but large amount should do.", Public = true },
                    new Configuration { DisplayPriority = 3, Category = "Judging", Name = "script_memory_limit", Value = "2097152", Type = "int", Description = "Maximum memory usage (in kB) by compile/compare scripts. This is a safeguard against malicious code and buggy script, so a reasonable but large amount should do.", Public = true },
                    new Configuration { DisplayPriority = 4, Category = "Judging", Name = "script_filesize_limit", Value = "540672", Type = "int", Description = "Maximum filesize (in kB) compile/compare scripts may write. Submission will fail with compiler-error when trying to write more, so this should be greater than any *intermediate or final* result written by compilers.", Public = true },
                    new Configuration { DisplayPriority = 5, Category = "Judging", Name = "timelimit_overshoot", Value = "\"1s|10%\"", Type = "string", Description = "Time that submissions are kept running beyond timelimit before being killed. Specify as \"Xs\" for X seconds, \"Y%\" as percentage, or a combination of both separated by one of \"+|&\" for the sum, maximum, or minimum of both.", Public = true },
                    new Configuration { DisplayPriority = 6, Category = "Judging", Name = "output_storage_limit", Value = "60000", Type = "int", Description = "Maximum size of error/system output stored in the database (in bytes); use \"-1\" to disable any limits.", Public = true },
                    new Configuration { DisplayPriority = 7, Category = "Judging", Name = "diskspace_error", Value = "1048576", Type = "int", Description = "Minimum free disk space (in kB) on judgehosts.", Public = true },
                    new Configuration { DisplayPriority = 8, Category = "Judging", Name = "update_judging_seconds", Value = "0", Type = "int", Description = "Post updates to a judging every X seconds. Set to 0 to update after each judging_run.", Public = true });
            });

            builder.Entity<Role>(entity =>
            {
                entity.HasData(Md.HasRoles);
            });

            builder.Entity<IdentityRoleClaim<int>>(entity =>
            {
                entity.HasData(Md.RoleClaims
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

            builder.Entity<User>(entity =>
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

            builder.Entity<IdentityUserRole<int>>(entity =>
            {
                entity.HasData(
                    new IdentityUserRole<int> { RoleId = Md.OfRole("Judgehost"), UserId = -1 });
            });
        }
    }
}
