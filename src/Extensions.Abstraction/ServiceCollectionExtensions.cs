using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
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

        /// <summary>
        /// Ensure the service has been registered.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="serviceLifetime">The service lifetime.</param>
        private static void EnsureRegistered<TService>(IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var serviceType = typeof(TService);

            for (int i = 0; i < services.Count; i++)
            {
                if (services[i].ServiceType != serviceType) continue;
                if (services[i].Lifetime != serviceLifetime)
                    throw new InvalidOperationException(
                        $"The service lifetime for {serviceType} is not correct.");
                return;
            }

            throw new InvalidOperationException(
                $"No implementation for {serviceType} was registered.");
        }

        /// <summary>
        /// Checks whether a scoped service of the type <typeparamref name="TService"/> has
        /// been registered in <see cref="IServiceCollection"/>. If not registered, throws an exception.
        /// </summary>
        /// <typeparam name="TService">The type of the service to ensure.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to check the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection EnsureScoped<TService>(this IServiceCollection services) where TService : class
        {
            EnsureRegistered<TService>(services, ServiceLifetime.Scoped);
            return services;
        }

        /// <summary>
        /// Checks whether a transient service of the type <typeparamref name="TService"/> has
        /// been registered in <see cref="IServiceCollection"/>. If not registered, throws an exception.
        /// </summary>
        /// <typeparam name="TService">The type of the service to ensure.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to check the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection EnsureTransient<TService>(this IServiceCollection services) where TService : class
        {
            EnsureRegistered<TService>(services, ServiceLifetime.Transient);
            return services;
        }

        /// <summary>
        /// Checks whether a singleton service of the type <typeparamref name="TService"/> has
        /// been registered in <see cref="IServiceCollection"/>. If not registered, throws an exception.
        /// </summary>
        /// <typeparam name="TService">The type of the service to ensure.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to check the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection EnsureSingleton<TService>(this IServiceCollection services) where TService : class
        {
            EnsureRegistered<TService>(services, ServiceLifetime.Singleton);
            return services;
        }

        /// <summary>
        /// Adds the support of <see cref="Lazy{T}"/> with <see cref="ServiceLifetime.Scoped"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddLazyScoped(this IServiceCollection services)
        {
            services.TryAdd(ServiceDescriptor.Scoped(typeof(Lazy<>), typeof(ServiceProviderLazyService<>)));
            return services;
        }

        /// <summary>
        /// The lazy support by <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        private class ServiceProviderLazyService<T> : Lazy<T>
        {
            public ServiceProviderLazyService(IServiceProvider sp) : base(sp.GetRequiredService<T>)
            {
            }
        }
    }
}
