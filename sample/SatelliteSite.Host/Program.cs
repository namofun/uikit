using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SatelliteSite.IdentityModule.Entities;

namespace SatelliteSite
{
    public class Program
    {
        public static IHost Current { get; private set; }

        public static void Main(string[] args)
        {
            Current = CreateHostBuilder(args).Build();
            Current.AutoMigrate<DefaultContext>();
            Current.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .MarkDomain<Program>()
                .AddModule<IdentityModule.IdentityModule<User, Role, DefaultContext>>()
                .AddModule<SampleModule.SampleModule>()
                .AddDatabaseMssql<DefaultContext>("UserDbConnection")
                .ConfigureSubstrateDefaults<DefaultContext>();
    }
}
