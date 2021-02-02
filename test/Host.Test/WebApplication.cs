using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SatelliteSite.IdentityModule.Entities;
using System.Reflection;

namespace SatelliteSite.Tests
{
    public class WebApplication : SubstrateApplicationBase
    {
        protected override Assembly EntryPointAssembly => typeof(DefaultContext).Assembly;

        protected override IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .MarkTest(this)
                .AddModule<IdentityModule.IdentityModule<User, Role, DefaultContext>>()
                .AddModule<SampleModule.SampleModule>()
                .AddDatabase<DefaultContext>(b => b.UseInMemoryDatabase("0x8c", b => b.UseBulk()))
                .ConfigureSubstrateDefaults<DefaultContext>();

        protected override void PrepareHost(IHost host) =>
            host.EnsureCreated<DefaultContext>();

        protected override void CleanupHost(IHost host) =>
            host.EnsureDeleted<DefaultContext>();
    }
}
