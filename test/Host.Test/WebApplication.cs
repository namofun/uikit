﻿using Microsoft.AspNetCore.Mvc;
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
                .MarkDomain<Program>()
                .AddModule<IdentityModule.IdentityModule<User, Role, DefaultContext>>()
                .AddModule<SampleModule.SampleModule>()
                .AddDatabaseInMemory<DefaultContext>("0x8c")
                .ConfigureSubstrateDefaults<DefaultContext>();

        protected override void PrepareHost(IHost host) =>
            host.EnsureCreated<DefaultContext>();

        protected override void CleanupHost(IHost host) =>
            host.EnsureDeleted<DefaultContext>();
    }
}
