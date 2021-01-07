using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SatelliteSite.IdentityModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SatelliteSite.IdentityModule
{
    public class IdentityEntityConfiguration<TUser, TRole, TContext> :
        IDbModelSupplier<TContext>
        where TUser : User, new()
        where TRole : Role, new()
        where TContext : IdentityDbContext<TUser, TRole, int>
    {
        public void Configure(EntityTypeBuilder<TUser> entity)
        {
            entity.Property(u => u.NickName)
                .HasMaxLength(256);
        }

        public void Configure(EntityTypeBuilder<TRole> entity)
        {
            var attributes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(e => e.GetCustomAttributes())
                .OfType<RoleDefinitionAttribute>()
                .ToList();

            var lookup = attributes.ToLookup(k => k.NormalizedName);
            var roles = new List<TRole>();

            foreach (var item in lookup)
            {
                var single = item.First();

                foreach (var replication in item)
                {
                    if (replication.Id != single.Id ||
                        replication.ShortName != single.ShortName ||
                        replication.Description != single.Description)
                    {
                        throw new ArgumentException(
                            "There are two roles defined the same name `" + single.Name + "` " +
                            "but not the same description / short name / id. " +
                            "You should keep them the same if they are actually the same role, " +
                            "or you should make the name unique.");
                    }
                }

                roles.Add(new TRole
                {
                    Id = -single.Id,
                    ConcurrencyStamp = single.GenerateDefaultConcurrencyStamp(),
                    Description = single.Description,
                    Name = single.Name,
                    NormalizedName = single.NormalizedName,
                    ShortName = single.ShortName,
                });
            }

            var idConflict = roles.GroupBy(r => r.Id).Where(g => g.Count() > 1).ToList();
            if (idConflict.Count > 0)
            {
                throw new ArgumentException(
                    "There are roles defined the same ID but not the same name. Conflicts are :\n" +
                    string.Join('\n', idConflict.Select(g => $"- {g.Key} : " + string.Join(", ", g.Select(r => r.Name)))));
            }

            entity.HasData(roles);
        }

        public void Configure(ModelBuilder builder)
        {
            builder.Entity<TUser>(Configure);

            builder.Entity<TRole>(Configure);
        }
    }
}
