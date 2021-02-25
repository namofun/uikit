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

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TElemenet">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented by <paramref name="keySelector"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{TSource}"/> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="ascending">True if ascending, false if descending.</param>
        /// <returns>An <see cref="IOrderedEnumerable{TSource}"/> whose elements are sorted according to a key.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is null.</exception>
        public static IOrderedEnumerable<TElemenet> OrderByBoolean<TElemenet, TKey>(
            this IEnumerable<TElemenet> source,
            Func<TElemenet, TKey> keySelector,
            bool ascending)
        {
            return ascending
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);
        }

        /// <summary>
        /// Performs a subsequent ordering of the elements in a sequence in ascending or descending order according to a key.
        /// </summary>
        /// <typeparam name="TElemenet">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented by <paramref name="keySelector"/>.</typeparam>
        /// <param name="source">An <see cref="IOrderedEnumerable{TElemenet}"/> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="ascending">True if ascending, false if descending.</param>
        /// <returns>An <see cref="IOrderedEnumerable{TElemenet}"/> whose elements are sorted according to a key.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is null.</exception>
        public static IOrderedEnumerable<TElemenet> ThenByBoolean<TElemenet, TKey>(
            this IOrderedEnumerable<TElemenet> source,
            Func<TElemenet, TKey> keySelector,
            bool ascending)
        {
            return ascending
                ? source.ThenBy(keySelector)
                : source.ThenByDescending(keySelector);
        }
    }
}
