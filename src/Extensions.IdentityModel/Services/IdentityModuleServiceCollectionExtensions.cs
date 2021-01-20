using Microsoft.AspNetCore.Identity;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityModuleServiceCollectionExtensions
    {
        /// <summary>
        /// Configure the <see cref="IdentityAdvancedOptions"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="setupAction">The option configure action.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection ConfigureIdentityAdvanced(this IServiceCollection services, Action<IdentityAdvancedOptions> setupAction)
        {
            return services.Configure(setupAction);
        }
    }
}
