using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// The model configuration supplier for a <see cref="DbContext"/>.
    /// </summary>
    /// <typeparam name="TContext">The <see cref="DbContext"/> to configure.</typeparam>
    public interface IDbModelSupplier<TContext>
        where TContext : DbContext
    {
        /// <summary>
        /// Configure the <see cref="ModelBuilder"/>.
        /// </summary>
        /// <param name="builder">The model builder.</param>
        void Configure(ModelBuilder builder);
    }

    /// <summary>
    /// The model configuration supplier for a <see cref="DbContext"/> by reading all <see cref="IEntityTypeConfiguration{TEntity}"/> methods.
    /// </summary>
    /// <typeparam name="TContext">The <see cref="DbContext"/> to configure.</typeparam>
    public abstract class EntityTypeConfigurationSupplier<TContext>
        : IDbModelSupplier<TContext>
        where TContext : DbContext
    {
        /// <summary>
        /// The generic type of <see cref="IEntityTypeConfiguration{TEntity}"/>
        /// </summary>
        static readonly Type GenericType = typeof(IEntityTypeConfiguration<>);

        /// <summary>
        /// The <see cref="ModelBuilder.ApplyConfiguration{TEntity}(IEntityTypeConfiguration{TEntity})"/> method.
        /// </summary>
        static readonly MethodInfo ApplyConfiguration =
            typeof(ModelBuilder).GetMethods()
            .Where(m => m.Name == nameof(ApplyConfiguration))
            .Where(m => IsEntityTypeConfiguration(m.GetParameters().FirstOrDefault().ParameterType))
            .Single();

        /// <summary>
        /// Check whether <paramref name="type"/> is constructed <see cref="IEntityTypeConfiguration{TEntity}"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The result.</returns>
        private static bool IsEntityTypeConfiguration(Type type)
        {
            if (!type.IsConstructedGenericType) return false;
            return type.GetGenericTypeDefinition() == GenericType;
        }

        /// <inheritdoc />
        public void Configure(ModelBuilder builder)
        {
            var interfaces = GetType().GetInterfaces();
            foreach (var entityConfig in interfaces.Where(IsEntityTypeConfiguration))
            {
                var entityType = entityConfig.GetGenericArguments().Single();
                var callingMethod = ApplyConfiguration.MakeGenericMethod(entityType);
                callingMethod.Invoke(builder, new object[] { this });
            }
        }
    }

    /// <summary>
    /// The extension class for model builders.
    /// </summary>
    public static class EntityTypeConfigurationSupplierExtensions
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
