#nullable disable
using System.Collections.Generic;

namespace System.Linq
{
    /// <summary>
    /// The <see cref="IEnumerable{T}"/> extensions from Substrate.
    /// </summary>
    public static class SubstrateEnumerableExtensions
    {
        /// <summary>
        /// If the <paramref name="condition"/> satisfies, apply the <paramref name="predicate"/> to the <paramref name="enumerable"/>.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="enumerable">The source enumerable.</param>
        /// <param name="condition">The apply condition.</param>
        /// <param name="predicate">The filter expression.</param>
        /// <returns>The solved enumerable.</returns>
        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, bool condition, Func<T, bool> predicate)
        {
            return condition ? enumerable.Where(predicate) : enumerable;
        }

        /// <summary>
        /// If the <paramref name="condition"/> satisfies, apply the <paramref name="selector"/> to the <paramref name="enumerable"/>.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="enumerable">The source enumerable.</param>
        /// <param name="condition">The apply condition.</param>
        /// <param name="selector">The selector expression.</param>
        /// <returns>The solved enumerable.</returns>
        public static IEnumerable<T> SelectIf<T>(this IEnumerable<T> enumerable, bool condition, Func<T, T> selector)
        {
            return condition ? enumerable.Select(selector) : enumerable;
        }

        /// <summary>
        /// If the skip count is not null, skip the corresponding count.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="enumerable">The source enumerable.</param>
        /// <param name="skipCount">The count to skip.</param>
        /// <returns>The solved enumerable.</returns>
        public static IEnumerable<T> SkipIf<T>(this IEnumerable<T> enumerable, int? skipCount)
        {
            if (!skipCount.HasValue) return enumerable;
            if (skipCount.Value >= 0) return enumerable.Skip(skipCount.Value);
            throw new InvalidOperationException("Skip count cannot be negative!");
        }

        /// <summary>
        /// If the take count is not null, take the corresponding count.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="enumerable">The source enumerable.</param>
        /// <param name="takeCount">The count to take.</param>
        /// <returns>The solved enumerable.</returns>
        public static IEnumerable<T> TakeIf<T>(this IEnumerable<T> enumerable, int? takeCount)
        {
            if (!takeCount.HasValue) return enumerable;
            if (takeCount.Value >= 0) return enumerable.Take(takeCount.Value);
            throw new InvalidOperationException("Take count cannot be negative!");
        }
    }
}
