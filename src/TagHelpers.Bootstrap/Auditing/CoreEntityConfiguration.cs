using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SatelliteSite.Entities
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

            entity.HasData(
                new Configuration { DisplayPriority = 1, Category = "Judging", Name = "process_limit", Value = "64", Type = "int", Description = "Maximum number of processes that the submission is allowed to start (including shell and possibly interpreters).", Public = true },
                new Configuration { DisplayPriority = 2, Category = "Judging", Name = "script_timelimit", Value = "30", Type = "int", Description = "Maximum seconds available for compile/compare scripts. This is a safeguard against malicious code and buggy scripts, so a reasonable but large amount should do.", Public = true },
                new Configuration { DisplayPriority = 3, Category = "Judging", Name = "script_memory_limit", Value = "2097152", Type = "int", Description = "Maximum memory usage (in kB) by compile/compare scripts. This is a safeguard against malicious code and buggy script, so a reasonable but large amount should do.", Public = true },
                new Configuration { DisplayPriority = 4, Category = "Judging", Name = "script_filesize_limit", Value = "540672", Type = "int", Description = "Maximum filesize (in kB) compile/compare scripts may write. Submission will fail with compiler-error when trying to write more, so this should be greater than any *intermediate or final* result written by compilers.", Public = true },
                new Configuration { DisplayPriority = 5, Category = "Judging", Name = "timelimit_overshoot", Value = "\"1s|10%\"", Type = "string", Description = "Time that submissions are kept running beyond timelimit before being killed. Specify as \"Xs\" for X seconds, \"Y%\" as percentage, or a combination of both separated by one of \"+|&\" for the sum, maximum, or minimum of both.", Public = true },
                new Configuration { DisplayPriority = 6, Category = "Judging", Name = "output_storage_limit", Value = "60000", Type = "int", Description = "Maximum size of error/system output stored in the database (in bytes); use \"-1\" to disable any limits.", Public = true },
                new Configuration { DisplayPriority = 7, Category = "Judging", Name = "diskspace_error", Value = "1048576", Type = "int", Description = "Minimum free disk space (in kB) on judgehosts.", Public = true },
                new Configuration { DisplayPriority = 8, Category = "Judging", Name = "update_judging_seconds", Value = "0", Type = "int", Description = "Post updates to a judging every X seconds. Set to 0 to update after each judging_run.", Public = true });
        }
    }
}
