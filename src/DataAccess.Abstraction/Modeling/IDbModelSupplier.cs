namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// The model configuration supplier for a <see cref="DbContext"/>.
    /// </summary>
    /// <typeparam name="TContext">The <see cref="DbContext"/> to configure.</typeparam>
    public interface IDbModelSupplier<TContext> where TContext : DbContext
    {
        /// <summary>
        /// Configure the <see cref="ModelBuilder"/>.
        /// </summary>
        /// <param name="builder">The model builder.</param>
        /// <param name="context">The <typeparamref name="TContext"/> instance being configured.</param>
        void Configure(ModelBuilder builder, TContext context);
    }
}
