using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace System.Linq
{
    /// <summary>
    /// The <see cref="IQueryable{T}"/> extensions from Substrate.
    /// </summary>
    public static class SubstrateQueryableExtensions
    {
        /// <summary>
        /// If the <paramref name="condition"/> satisfies, apply the <paramref name="predicate"/> to the <paramref name="queryable"/>.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="queryable">The source queryable.</param>
        /// <param name="condition">The apply condition.</param>
        /// <param name="predicate">The filter expression.</param>
        /// <returns>The solved queryable.</returns>
        public static IQueryable<T> WhereIf<T>(
            this IQueryable<T> queryable,
            bool condition,
            Expression<Func<T, bool>> predicate)
        {
            return condition ? queryable.Where(predicate) : queryable;
        }

        /// <summary>
        /// If the <paramref name="condition"/> satisfies, apply the <paramref name="inclusion"/> to the <paramref name="queryable"/>.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="T2">The included entity type.</typeparam>
        /// <param name="queryable">The source queryable.</param>
        /// <param name="condition">The apply condition.</param>
        /// <param name="inclusion">The inclusion expression.</param>
        /// <returns>The solved queryable.</returns>
        public static IQueryable<T> IncludeIf<T, T2>(
            this IQueryable<T> queryable,
            bool condition,
            Expression<Func<T, T2>> inclusion)
            where T : class
        {
            return condition ? queryable.Include(inclusion) : queryable;
        }
    }
}
