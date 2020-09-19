using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <inheritdoc />
    public class SuppliedModelCustomizer<T> : RelationalModelCustomizer where T : DbContext
    {
        /// <summary>
        /// The holder list
        /// </summary>
        public static List<IDbModelSupplier<T>> Holder { get; } = new List<IDbModelSupplier<T>>();

        /// <inheritdoc />
        public SuppliedModelCustomizer(ModelCustomizerDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            foreach (var supplier in Holder)
            {
                supplier.Configure(modelBuilder);
            }
        }
    }
}
