using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore
{
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
            .Where(m => IsEntityTypeConfiguration(m.GetParameters()[0].ParameterType))
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
}
