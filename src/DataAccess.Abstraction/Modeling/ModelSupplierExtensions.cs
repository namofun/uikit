using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// The extension class for model builders.
    /// </summary>
    public static class ModelSupplierExtensions
    {
        /// <summary>
        /// The class for holding extensions.
        /// </summary>
        /// <typeparam name="T">The <see cref="DbContext"/>.</typeparam>
        private class Supplier<T> where T : DbContext
        {
            /// <summary>
            /// The holder list
            /// </summary>
            public static List<IDbModelSupplier<T>> Holder { get; } = new List<IDbModelSupplier<T>>();
        }

        /// <summary>
        /// Apply the DbContext configuration.
        /// </summary>
        /// <typeparam name="TContext">The DbContext type.</typeparam>
        /// <param name="supplierSource">The supplier source.</param>
        /// <param name="builder"></param>
        public static void FromSupplierToModelBuilder<TContext>(
            this TContext supplierSource,
            ModelBuilder builder)
            where TContext : DbContext
        {
            foreach (var supplier in Supplier<TContext>.Holder)
            {
                supplier.Configure(builder);
            }
        }

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
            Supplier<TContext>.Holder.Add(new TSupplier());
            return services;
        }
    }
}
