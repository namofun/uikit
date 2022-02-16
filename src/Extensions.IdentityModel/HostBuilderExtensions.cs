using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Hosting
{
    public static class IdentityModuleHostBuilderExtensions
    {
        public static IHostBuilder EnableIdentityModuleBasicAuthentication(this IHostBuilder builder)
        {
            var modules = (List<AbstractModule>)builder.Properties["Substrate.Modules"];
            var options = modules.OfType<IIdentityModuleOptions>().SingleOrDefault();
            if (options == null)
            {
                throw new InvalidOperationException("Identity module not registered.");
            }

            options.EnableBasicAuthentication = true;
            return builder;
        }

        public static IHostBuilder EnableIdentityModuleJwtAuthentication(this IHostBuilder builder)
        {
            var modules = (List<AbstractModule>)builder.Properties["Substrate.Modules"];
            var options = modules.OfType<IIdentityModuleOptions>().SingleOrDefault();
            if (options == null)
            {
                throw new InvalidOperationException("Identity module not registered.");
            }

            options.EnableJwtAuthentication = true;
            return builder;
        }
    }
}
