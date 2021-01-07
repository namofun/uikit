﻿namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <inheritdoc />
    public class SuppliedModelCustomizer<T> : ModelCustomizer where T : DbContext
    {
        /// <inheritdoc />
        public SuppliedModelCustomizer(ModelCustomizerDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            var service = context.GetService<IDbContextOptions>()
                .FindExtension<ModelSupplierService<T>>();

            foreach (var supplier in service.Holder)
                supplier.Configure(modelBuilder);

            base.Customize(modelBuilder, context);
        }
    }
}
