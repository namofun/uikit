using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;

namespace System
{
    internal static class SequentialGuidGeneratorHelper
    {
        internal static SequentialGuidType GetSequentialGuidType(DbContextOptions options)
        {
            if (options.Extensions.Where(e => e.Info.IsDatabaseProvider).FirstOrDefault() is IDbContextOptionsExtension dbext
                && SequentialGuidGenerator.DatabaseMapping.TryGetValue(dbext.GetType().Assembly.GetName().Name ?? "", out var relGuidSeq))
            {
                return relGuidSeq;
            }
            else
            {
                throw new InvalidOperationException("Unknown database type configured.");
            }
        }
    }

    /// <summary>
    /// Generate sequential GUID for a certain DbContext.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    public class SequentialGuidGenerator<TContext> : SequentialGuidGenerator where TContext : DbContext
    {
        /// <summary>
        /// Initialize the generator.
        /// </summary>
        /// <param name="options">The DbContextOptions.</param>
        public SequentialGuidGenerator(DbContextOptions<TContext> options)
            : base(SequentialGuidGeneratorHelper.GetSequentialGuidType(options))
        {
        }
    }
}
