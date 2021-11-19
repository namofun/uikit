#nullable disable
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("SatelliteSite.DataAccess.Tests")]

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
        /// If the <paramref name="condition"/> satisfies, apply the <paramref name="selector"/> to the <paramref name="queryable"/>.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="queryable">The source queryable.</param>
        /// <param name="condition">The apply condition.</param>
        /// <param name="selector">The selector expression.</param>
        /// <returns>The solved queryable.</returns>
        public static IQueryable<T> SelectIf<T>(
            this IQueryable<T> queryable,
            bool condition,
            Expression<Func<T, T>> selector)
        {
            return condition ? queryable.Select(selector) : queryable;
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

        /// <summary>
        /// If the skip count is not null, skip the corresponding count.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="queryable">The source queryable.</param>
        /// <param name="skipCount">The count to skip.</param>
        /// <returns>The solved queryable.</returns>
        public static IQueryable<T> SkipIf<T>(
            this IQueryable<T> queryable,
            int? skipCount)
        {
            if (!skipCount.HasValue) return queryable;
            if (skipCount.Value >= 0) return queryable.Skip(skipCount.Value);
            throw new InvalidOperationException("Skip count cannot be negative!");
        }

        /// <summary>
        /// If the take count is not null, take the corresponding count.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="queryable">The source queryable.</param>
        /// <param name="takeCount">The count to take.</param>
        /// <returns>The solved queryable.</returns>
        public static IQueryable<T> TakeIf<T>(
            this IQueryable<T> queryable,
            int? takeCount)
        {
            if (!takeCount.HasValue) return queryable;
            if (takeCount.Value >= 0) return queryable.Take(takeCount.Value);
            throw new InvalidOperationException("Take count cannot be negative!");
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented by <paramref name="keySelector"/>.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}"/> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="ascending">True if ascending, false if descending.</param>
        /// <returns>An <see cref="IOrderedQueryable{TSource}"/> whose elements are sorted according to a key.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is null.</exception>
        public static IOrderedQueryable<TSource> OrderByBoolean<TSource, TKey>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            bool ascending)
        {
            return ascending
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);
        }

        /// <summary>
        /// Performs a subsequent ordering of the elements in a sequence in ascending or descending order according to a key.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented by <paramref name="keySelector"/>.</typeparam>
        /// <param name="source">An <see cref="IOrderedQueryable{TSource}"/> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="ascending">True if ascending, false if descending.</param>
        /// <returns>An <see cref="IOrderedQueryable{TSource}"/> whose elements are sorted according to a key.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is null.</exception>
        public static IOrderedQueryable<TSource> ThenByBoolean<TSource, TKey>(
            this IOrderedQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            bool ascending)
        {
            return ascending
                ? source.ThenBy(keySelector)
                : source.ThenByDescending(keySelector);
        }

        /// <summary>
        /// Asynchronously creates an <see cref="IPagedList{T}"/> from the query source.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="queryable">The source queryable.</param>
        /// <param name="currentPage">The current page.</param>
        /// <param name="countPerPage">The count per page.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The task for fetching paged list.</returns>
        public static async Task<IPagedList<TSource>> ToPagedListAsync<TSource>(
            this IQueryable<TSource> queryable,
            int currentPage,
            int countPerPage,
            CancellationToken cancellationToken = default)
        {
            if (queryable == null) throw new ArgumentNullException(nameof(queryable));
            if (currentPage <= 0) throw new ArgumentOutOfRangeException(nameof(currentPage));
            if (countPerPage <= 0) throw new ArgumentOutOfRangeException(nameof(countPerPage));

            var count = await queryable.CountAsync(cancellationToken);
            var content = await queryable.Skip((currentPage - 1) * countPerPage).Take(countPerPage).ToListAsync(cancellationToken);
            return new PagedViewList<TSource>(content, currentPage, count, countPerPage);
        }

        /// <summary>
        /// Asynchronously creates a <see cref="Lookup{TKey, TElement}"/> from an <see cref="IQueryable{T}"/> according to specified key selector and element selector functions.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <typeparam name="TElement">The type of the element returned by <paramref name="elementSelector"/>.</typeparam>
        /// <param name="source">The <see cref="IQueryable{T}"/> to create a <see cref="ILookup{TKey, TElement}"/> from.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The task for an <see cref="ILookup{TKey, TElement}"/> that contains keys and values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> or <paramref name="elementSelector"/> is null.</exception>
        public static async Task<ILookup<TKey, TElement>> ToLookupAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken = default)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));

            var results = await source.ToListAsync(cancellationToken);
            return results.ToLookup(keySelector, elementSelector);
        }

        /// <summary>
        /// Asynchronously creates a <see cref="HashSet{T}"/> from an <see cref="IQueryable{T}"/>.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="IQueryable{T}"/> to create a <see cref="HashSet{T}"/> from.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The task for a <see cref="HashSet{T}"/> that contains keys and values.</returns>
        public static async Task<HashSet<TSource>> ToHashSetAsync<TSource>(
            this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            var hashSet = new HashSet<TSource>();
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                hashSet.Add(element);
            }

            return hashSet;
        }

        /// <summary>
        /// Get a transformed paged view list from an asynchronous fetching.
        /// </summary>
        /// <typeparam name="T">The source entity type.</typeparam>
        /// <typeparam name="T2">The target entity type.</typeparam>
        /// <param name="listTask">The task to fetch a paged view list.</param>
        /// <param name="transformer">The transform delegate.</param>
        /// <returns>The task for fetching paged list.</returns>
        public static async Task<IPagedList<T2>> TransformAfterAcquire<T, T2>(
            this Task<IPagedList<T>> listTask,
            Func<T, T2> transformer)
        {
            return (await listTask).As(transformer);
        }

        /// <summary>
        /// Get a transformed enumerable from an asynchronous fetching.
        /// </summary>
        /// <typeparam name="T">The source entity type.</typeparam>
        /// <typeparam name="T2">The target entity type.</typeparam>
        /// <param name="listTask">The task to fetch an enumerable.</param>
        /// <param name="transformer">The transform delegate.</param>
        /// <returns>The task for fetching enumerable.</returns>
        public static async Task<IEnumerable<T2>> TransformAfterAcquire<T, T2>(
            this Task<List<T>> listTask,
            Func<T, T2> transformer)
        {
            return (await listTask).Select(transformer);
        }

        /// <summary>
        /// Correlates the elements of two sequences based on key equality and groups the results. The default equality comparer is used to compare keys.
        /// </summary>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from an element from the first sequence and zero or one of matching elements from the second sequence.</param>
        /// <returns>An <see cref="IQueryable{TResult}"/> that has elements of type TResult obtained by performing a left join on two sequences.</returns>
        public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
            this IQueryable<TOuter> outer,
            IQueryable<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            return outer
                .GroupJoin(
                    inner, outerKeySelector, innerKeySelector,
                    (outer, inners) => new { outer, inners })
                .SelectMany(
                    a => a.inners.DefaultIfEmpty(),
                    resultSelector.Combine(new { outer = default(TOuter), inners = default(IEnumerable<TInner>) }, t1 => t1.outer));
        }
    }
}
