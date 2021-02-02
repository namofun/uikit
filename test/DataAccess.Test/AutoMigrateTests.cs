using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SatelliteSite.Entities;
using SatelliteSite.Services;
using System;
using System.Threading.Tasks;

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
            }
        }

        private static void Further(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddDbModelSupplier<Context, ContextMore>();
            });
        }

        private async Task EnsureDefaultEntities(Action<DbContextOptionsBuilder> configureAction)
        {
            var host = Host.CreateDefaultBuilder()
                .MarkTest()
                .AddDatabase<Context>(configureAction)
                .ConfigureSubstrateDefaults<Context>(Further)
                .Build();

            host.EnsureCreated<Context>();

            using (var scope = host.Services.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<Context>();
                var cache = scope.ServiceProvider.GetRequiredService<ConfigurationRegistryCache>();
                IConfigurationRegistry configurationRegistry = new ConfigurationRegistry<Context>(context, cache);
                Assert.IsNotNull(await configurationRegistry.FindAsync("conf_name"));
            }

            host.EnsureDeleted<Context>();
        }

        [TestMethod]
        public async Task EnsureDefaultEntitiesInMemory()
        {
            await EnsureDefaultEntities(b => b.UseInMemoryDatabase("0x8c", b => b.UseBulk()));
        }

        [TestMethod]
        [TestCategory("SqlServer")]
        public async Task EnsureDefaultEntitiesSqlServer()
        {
            var cns = "Server=(localdb)\\mssqllocaldb;" +
                      "Database=aspnet-UIKitTest" + Guid.NewGuid().ToString("D")[4] + ";" +
                      "Trusted_Connection=True;" +
                      "MultipleActiveResultSets=true";

            await EnsureDefaultEntities(b => b.UseSqlServer(cns, b => b.UseBulk()));
        }
    }
}
