using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for Substrate dependency injection.
    /// </summary>
    public static class SubstrateServiceCollectionExtensions
    {
        /// <summary>
        /// Replaces a scoped service of the type specified in <typeparamref name="TService"/>
        /// with an implementation type specified in <typeparamref name="TImplementation"/>
        /// to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to replace.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to replace the service at.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection ReplaceScoped<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            return services.Replace(ServiceDescriptor.Scoped<TService, TImplementation>());
        }
    }
}
