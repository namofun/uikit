using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

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

        /// <summary>
        /// Replaces a singleton service of the type specified in <typeparamref name="TService"/>
        /// with an implementation type specified in <typeparamref name="TImplementation"/>
        /// to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to replace.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to replace the service at.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection ReplaceSingleton<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            return services.Replace(ServiceDescriptor.Singleton<TService, TImplementation>());
        }

        /// <summary>
        /// Adds a singleton service of the type specified in <typeparamref name="TService"/>
        /// with an implementation by upcasting the type specified in <typeparamref name="TImplementation"/>
        /// to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddSingletonUpcast<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            return services.AddSingleton(sp => (TService)sp.GetRequiredService<TImplementation>());
        }

        /// <summary>
        /// Adds a scoped service of the type specified in <typeparamref name="TService"/>
        /// with an implementation by upcasting the type specified in <typeparamref name="TImplementation"/>
        /// to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddScopedUpcast<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            return services.AddScoped(sp => (TService)sp.GetRequiredService<TImplementation>());
        }

        /// <summary>
        /// Adds a singleton service of the type specified in <typeparamref name="TService"/>
        /// with an implementation by downcasting the type specified in <typeparamref name="TImplementation"/>
        /// to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddSingletonDowncast<TService, TImplementation>(this IServiceCollection services)
            where TService : class, TImplementation
            where TImplementation : class
        {
            return services.AddSingleton(sp => (TService)sp.GetRequiredService<TImplementation>());
        }

        /// <summary>
        /// Adds a scoped service of the type specified in <typeparamref name="TService"/>
        /// with an implementation by downcasting the type specified in <typeparamref name="TImplementation"/>
        /// to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddScopedDowncast<TService, TImplementation>(this IServiceCollection services)
            where TService : class, TImplementation
            where TImplementation : class
        {
            return services.AddScoped(sp => (TService)sp.GetRequiredService<TImplementation>());
        }

        /// <summary>
        /// Add the assembly to MediatR.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assembly">The handler assembly.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddMediatRAssembly(this IServiceCollection services, Assembly assembly)
        {
            MediatR.Registration.ServiceRegistrar.AddMediatRClasses(services, new[] { assembly });
            return services;
        }
    }
}
