using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SatelliteSite.Entities;
using System;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class AutoMigrateTests
    {
        private class Context : DbContext
        {
            public Context(DbContextOptions options) : base(options)
            {
            }
        }

        private class ContextMore :
            EntityTypeConfigurationSupplier<Context>,
            IEntityTypeConfiguration<Configuration>
        {
            public void Configure(EntityTypeBuilder<Configuration> builder)
            {
                var it = new ConfigurationStringAttribute(1, "1", "conf_name", "1", "1");
                builder.HasData(it.ToEntity());
                builder.HasKey(e => e.Name);
            }
        }

        [TestMethod]
        public void EnsureDefaultEntitiesInMemory()
        {
            using var host = Host.CreateDefaultBuilder()
                .AddDatabase<Context>(b => b.UseInMemoryDatabase("0x8c", b => b.UseBulk()))
                .ConfigureServices(services => services.AddDbModelSupplier<Context, ContextMore>())
                .Build()
                .EnsureCreated<Context>();

            using var scope = host.Services.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<Context>();

            Assert.IsNotNull(ctx.Set<Configuration>().Find("conf_name"));
        }

        [TestMethod]
        public void EnsureDefaultEntitiesSqlServer()
        {
            using var host = Host.CreateDefaultBuilder()
                .AddDatabase<Context>(b => b.UseSqlServer("a", b => b.UseBulk()))
                .ConfigureServices(services => services.AddDbModelSupplier<Context, ContextMore>())
                .Build();

            using var scope = host.Services.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<Context>();

            var script = ctx.Database.GenerateCreateScript();

            string shouldHave =
                "INSERT INTO [Configuration] ([Name], [Category], [Description], [DisplayPriority], [Public], [Type], [Value])" +
                Environment.NewLine +
                "VALUES (N'conf_name', N'1', N'1', 1, CAST(1 AS bit), N'string', N'\"1\"');";

            Assert.IsTrue(script.Contains(shouldHave));
        }

        [TestMethod]
        public void EnsureDefaultEntitiesNpgsql()
        {
            using var host = Host.CreateDefaultBuilder()
                .AddDatabase<Context>(b => b.UseNpgsql("a", b => b.UseBulk()))
                .ConfigureServices(services => services.AddDbModelSupplier<Context, ContextMore>())
                .Build();

            using var scope = host.Services.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<Context>();

            var script = ctx.Database.GenerateCreateScript();

            string shouldHave =
                "INSERT INTO \"Configuration\" (\"Name\", \"Category\", \"Description\", \"DisplayPriority\", \"Public\", \"Type\", \"Value\")" +
                Environment.NewLine +
                "VALUES ('conf_name', '1', '1', 1, TRUE, 'string', '\"1\"');";

            Assert.IsTrue(script.Contains(shouldHave));
        }
    }
}
