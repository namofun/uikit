using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// The extension class for model builders.
    /// </summary>
    public static class ModelSupplierExtensions
    {
        /// <summary>
        /// Add the <typeparamref name="TSupplier"/> to dependency injection container.
        /// </summary>
        /// <typeparam name="TContext">The <see cref="DbContext"/> to configure.</typeparam>
        /// <typeparam name="TSupplier">The <see cref="IDbModelSupplier{TContext}"/> to supply.</typeparam>
        /// <param name="services">The dependency injection container.</param>
        /// <returns>The dependency injection container.</returns>
        public static IServiceCollection AddDbModelSupplier<TContext, TSupplier>(this IServiceCollection services)
            where TContext : DbContext
            where TSupplier : class, IDbModelSupplier<TContext>, new()
        {
            services.AddSingleton<IDbModelSupplier<TContext>>(new TSupplier());
            return services;
        }
    }
}
