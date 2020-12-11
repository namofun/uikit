using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    /// The <see cref="IDbContextOptionsExtension"/> storing all <see cref="IDbModelSupplier{TContext}"/>,
    /// which will be used in <see cref="IModelCustomizer"/> soon.
    /// </summary>
    /// <typeparam name="T">The context type.</typeparam>
    public class ModelSupplierService<T> : IDbContextOptionsExtension
        where T : DbContext
    {
        /// <summary>
        /// The stored <see cref="IDbModelSupplier{TContext}"/>s.
        /// </summary>
        public IReadOnlyList<IDbModelSupplier<T>> Holder { get; }

        /// <summary>
        /// Construct a static <see cref="ModelSupplierService{T}"/>.
        /// </summary>
        /// <param name="suppliers">The suppliers to be used.</param>
        public ModelSupplierService(IEnumerable<IDbModelSupplier<T>> suppliers)
        {
            Holder = suppliers.ToArray();
            Info = new ModelSupplierExtensionInfo(this);
        }

        /// <inheritdoc />
        public DbContextOptionsExtensionInfo Info { get; }

        /// <inheritdoc />
        public void ApplyServices(IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton<IModelCustomizer, SuppliedModelCustomizer<T>>());
        }

        /// <inheritdoc />
        public void Validate(IDbContextOptions options)
        {
        }

        /// <inheritdoc />
        private class ModelSupplierExtensionInfo : DbContextOptionsExtensionInfo
        {
            /// <inheritdoc cref="DbContextOptionsExtensionInfo.Extension" />
            public new ModelSupplierService<T> Extension { get; }

            /// <summary>
            /// Creates a new <see cref="DbContextOptionsExtensionInfo"/> instance containing info/metadata for the given extension.
            /// </summary>
            /// <param name="extension">The extension.</param>
            public ModelSupplierExtensionInfo(ModelSupplierService<T> extension) : base(extension)
            {
                Extension = extension;
            }

            /// <inheritdoc />
            public override bool IsDatabaseProvider => false;

            /// <inheritdoc />
            public override string LogFragment => string.Empty;

            /// <inheritdoc />
            public override long GetServiceProviderHashCode() => 0L;

            /// <inheritdoc />
            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
                => debugInfo["DbModelSupplier"] = "Count=" + Extension.Holder.Count;
        }
    }
}
