using Jobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SatelliteSite.IdentityModule.Entities;

namespace SatelliteSite.JobsModule
{
    public class JobsEntityConfiguration<TUser, TContext> :
        EntityTypeConfigurationSupplier<TContext>,
        IEntityTypeConfiguration<Job>
        where TUser : User, new()
        where TContext : DbContext
    {
        public void Configure(EntityTypeBuilder<Job> entity)
        {
            entity.ToTable("Jobs");

            entity.HasKey(e => e.JobId);

            entity.HasOne<Job>()
                .WithMany()
                .HasForeignKey(e => e.ParentJobId)
                .HasPrincipalKey(e => e.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<TUser>()
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CreationTime);

            entity.HasIndex(e => e.Status);
        }
    }
}
