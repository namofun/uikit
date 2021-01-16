using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for Substrate dependency injection.
    /// </summary>
    public static class SubstrateConfigureServiceCollectionExtensions
    {
        /// <summary>
        /// Registers an action used to configure <see cref="MvcOptions"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection ConfigureMvc(this IServiceCollection services, Action<MvcOptions> configureOptions)
        {
            return services.Configure<MvcOptions>(configureOptions);
        }

        /// <summary>
        /// Registers an action used to configure <see cref="RouteOptions"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection ConfigureRouting(this IServiceCollection services, Action<RouteOptions> configureOptions)
        {
            return services.Configure<RouteOptions>(configureOptions);
        }

        /// <summary>
        /// Registers an action used to configure <see cref="SubstrateOptions"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection ConfigureApplicationBuilder(this IServiceCollection services, Action<SubstrateOptions> configureOptions)
        {
            return services.Configure<SubstrateOptions>(configureOptions);
        }
    }
}
