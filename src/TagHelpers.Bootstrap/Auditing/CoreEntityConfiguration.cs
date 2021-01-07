using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SatelliteSite.Entities;
using System;
using System.Linq;
using System.Reflection;

namespace SatelliteSite.Substrate
{
    public class CoreEntityConfiguration<TContext> :
        EntityTypeConfigurationSupplier<TContext>,
        IEntityTypeConfiguration<Auditlog>,
        IEntityTypeConfiguration<Configuration>
        where TContext : DbContext
    {
        public void Configure(EntityTypeBuilder<Auditlog> entity)
        {
            entity.HasKey(e => e.LogId);

            entity.ToTable("Auditlogs");

            entity.Property(e => e.UserName)
                .IsRequired();

            entity.HasIndex(e => e.DataType);

            entity.Property(e => e.DataId)
                .HasMaxLength(256)
                .IsUnicode(false);

            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(256)
                .IsUnicode(false);
        }

        public void Configure(EntityTypeBuilder<Configuration> entity)
        {
            entity.HasKey(e => e.Name);

            entity.ToTable("Configurations");

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

            var attributes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(e => e.GetCustomAttributes())
                .OfType<ConfigurationItemAttribute>()
                .Select(e => e.ToEntity())
                .ToList();

            entity.HasData(attributes);
        }
    }
}
