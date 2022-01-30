using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class ModelCustomizerTests
    {
        private class Context : DbContext
        {
            public Context(DbContextOptions options) : base(options)
            {
            }
        }

        private class Entity
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private class EntityTypeConfigurationSupplier :
            EntityTypeConfigurationSupplier<Context>,
            IEntityTypeConfiguration<Entity>
        {
            private readonly Guid _guid;
            private readonly int _id;

            public EntityTypeConfigurationSupplier(int id, Guid guid)
            {
                _id = id;
                _guid = guid;
            }

            public void Configure(EntityTypeBuilder<Entity> builder)
            {
                builder.HasData(new Entity { Id = _id, Name = _guid.ToString() });
            }
        }

        private class GeneralDbModelSupplier :
            IDbModelSupplier<Context>
        {
            public bool Executed { get; private set; }

            public void Configure(ModelBuilder builder, Context context)
            {
                Executed = true;
            }
        }

        [TestMethod]
        public void CheckSupplierInContainer()
        {
            // arrange
            const int EntityCount = 10;

            var list = Enumerable.Repeat(1, EntityCount)
                .Select((_, i) => (i: -1-i, v: Guid.NewGuid()))
                .ToDictionary(k => k.i, k => k.v);

            using var host = new SimpleHostBuilder()
                .AddDatabase<Context>(b => b.UseInMemoryDatabase("0x3f", b => b.UseBulk()))
                .ConfigureServices(services =>
                {
                    services.AddDbModelSupplier<Context, GeneralDbModelSupplier>();
                    foreach (var item in list)
                    {
                        services.AddSingleton<IDbModelSupplier<Context>>(new EntityTypeConfigurationSupplier(item.Key, item.Value));
                    }
                })
                .Build();

            // pre-act assert
            {
                var svc = host.Services.GetRequiredService<ModelSupplierService<Context>>();
                Assert.AreEqual(1 + EntityCount, svc.Holder.Count);
                var ctx = svc.Holder.OfType<GeneralDbModelSupplier>().Single();
                Assert.IsFalse(ctx.Executed);
            }

            // act
            using (var scope = host.Services.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<Context>();
                var model = context.GetService<IDesignTimeModel>().Model;
                var types = model.GetEntityTypes().ToList();
                Assert.AreEqual(1, types.Count);
                var entityType = types[0];

                var seeds = entityType.GetSeedData().ToList();
                Assert.AreEqual(EntityCount, seeds.Count);

                foreach (var item in seeds)
                {
                    Assert.IsTrue(item.ContainsKey("Id"));
                    Assert.IsTrue(item.ContainsKey("Name"));
                    Assert.IsInstanceOfType(item["Id"], typeof(int));
                    Assert.IsInstanceOfType(item["Name"], typeof(string));
                    int id = (int)item["Id"];
                    Assert.IsTrue(list.ContainsKey(id));
                    Assert.AreEqual(list[id].ToString(), (string)item["Name"]);
                }
            }

            // post-act assert
            {
                var svc = host.Services.GetRequiredService<ModelSupplierService<Context>>();
                Assert.AreEqual(1 + EntityCount, svc.Holder.Count);
                var ctx = svc.Holder.OfType<GeneralDbModelSupplier>().Single();
                Assert.IsTrue(ctx.Executed);
            }
        }

        [TestMethod]
        public void CheckBulkRegistered()
        {
            static void RegisterAndTest(Action<DbContextOptionsBuilder> action)
                => new SimpleHostBuilder()
                    .AddDatabase<Context>(action)
                    .Build()
                    .Services
                    .GetRequiredService<Context>();

            const string ConnectionString = "Host=localhost";

            RegisterAndTest(b => b.UseInMemoryDatabase(ConnectionString, b => b.UseBulk()));
            RegisterAndTest(b => b.UseSqlServer(ConnectionString, b => b.UseBulk()));
            RegisterAndTest(b => b.UseNpgsql(ConnectionString, b => b.UseBulk()));
            RegisterAndTest(b => b.UseSqlite(ConnectionString, b => b.UseBulk()));
            RegisterAndTest(b => b.UseSqlite(ConnectionString, b => b.UseBulk()));
            RegisterAndTest(b => b.UseMySql(ConnectionString, ServerVersion.Parse("8.0.21-mysql"), b => b.UseBulk()));

            Assert.ThrowsException<InvalidOperationException>(
                () => RegisterAndTest(b => b.UseInMemoryDatabase(ConnectionString)),
                HostBuilderDataAccessExtensions.NoBulkExtRegistered);

            Assert.ThrowsException<InvalidOperationException>(
                () => RegisterAndTest(b => b.UseSqlServer(ConnectionString)),
                HostBuilderDataAccessExtensions.NoBulkExtRegistered);

            Assert.ThrowsException<InvalidOperationException>(
                () => RegisterAndTest(b => b.UseNpgsql(ConnectionString)),
                HostBuilderDataAccessExtensions.NoBulkExtRegistered);

            Assert.ThrowsException<InvalidOperationException>(
                () => RegisterAndTest(b => b.UseSqlite(ConnectionString)),
                HostBuilderDataAccessExtensions.NoBulkExtRegistered);

            Assert.ThrowsException<InvalidOperationException>(
                () => RegisterAndTest(b => b.UseMySql(ConnectionString, ServerVersion.Parse("8.0.21-mysql"))),
                HostBuilderDataAccessExtensions.NoBulkExtRegistered);
        }

        [TestMethod]
        public void GetSequentialGuidType()
        {
            DbContextOptions RegisterAndTest(Action<DbContextOptionsBuilder> action)
                => new SimpleHostBuilder()
                    .AddDatabase<Context>(action)
                    .Build()
                    .Services
                    .GetRequiredService<DbContextOptions<Context>>();

            void Validate(Action<DbContextOptionsBuilder> action, SequentialGuidType type)
                => Assert.AreEqual(type, SequentialGuidGeneratorHelper.GetSequentialGuidType(RegisterAndTest(action)));

            const string ConnectionString = "Host=localhost";

            Validate(b => b.UseInMemoryDatabase(ConnectionString, b => b.UseBulk()), SequentialGuidType.SequentialAsString);
            Validate(b => b.UseSqlServer(ConnectionString, b => b.UseBulk()), SequentialGuidType.SequentialAtEnd);
            Validate(b => b.UseNpgsql(ConnectionString, b => b.UseBulk()), SequentialGuidType.SequentialAsString);
            Validate(b => b.UseSqlite(ConnectionString, b => b.UseBulk()), SequentialGuidType.SequentialAsString);
            Validate(b => b.UseMySql(ConnectionString, ServerVersion.Parse("8.0.21-mysql"), b => b.UseBulk()), SequentialGuidType.SequentialAsString);
            // ["Oracle.EntityFrameworkCore"] = SequentialGuidType.SequentialAsBinary
        }
    }
}
