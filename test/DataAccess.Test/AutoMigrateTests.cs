using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SatelliteSite.Entities;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
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

        private async Task EnsureDefaultEntities(
            Action<Dictionary<string, string>> moreConf,
            Func<IHostBuilder, IHostBuilder> addDatabase)
        {
            var dict = new Dictionary<string, string>
            {
            };

            moreConf?.Invoke(dict);

            var builder = Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(c => c.AddInMemoryCollection(dict));

            var host = addDatabase(builder.MarkTest())
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
            await EnsureDefaultEntities(
                c => { },
                b => b.AddDatabaseInMemory<Context>("0x8c"));
        }

        [TestMethod]
        [TestCategory("SqlServer")]
        public async Task EnsureDefaultEntitiesSqlServer()
        {
            var cns = "Server=(localdb)\\mssqllocaldb;" +
                      "Database=aspnet-UIKitTest" + Guid.NewGuid().ToString("D")[4] + ";" +
                      "Trusted_Connection=True;" +
                      "MultipleActiveResultSets=true";

            await EnsureDefaultEntities(
                c => c["ConnectionStrings:TestEnv"] = cns,
                b => b.AddDatabaseMssql<Context>("TestEnv"));
        }
    }
}
