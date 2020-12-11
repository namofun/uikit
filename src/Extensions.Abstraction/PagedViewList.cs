using System.Linq;

namespace System.Collections.Generic
{
    /// <summary>
    /// A paged <see cref="IReadOnlyList{T}"/> view.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    public interface IPagedList<out T> : IReadOnlyList<T>
    {
        /// <summary>
        /// Gets the total entity count.
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// Gets the total page count.
        /// </summary>
        int TotalPage { get; }

        /// <summary>
        /// Gets the count of each page.
        /// </summary>
        int CountPerPage { get; }

        /// <summary>
        /// Gets the current page.
        /// </summary>
        int CurrentPage { get; }
    }

    /// <summary>
    /// A paged <see cref="IReadOnlyList{T}"/> view.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    public class PagedViewList<T> : IPagedList<T>
    {
        /// <inheritdoc />
        public T this[int index] => Content[index];

        /// <inheritdoc />
        public int Count => Content.Count;

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => Content.GetEnumerator();

        /// <summary>
        /// Gets the total entity count.
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// Gets the total page count.
        /// </summary>
        public int TotalPage { get; }

        /// <summary>
        /// Gets the count of each page.
        /// </summary>
        public int CountPerPage { get; }

        /// <summary>
        /// Gets the current page.
        /// </summary>
        public int CurrentPage { get; }

        /// <summary>
        /// The internal list.
        /// </summary>
        internal IReadOnlyList<T> Content { get; set; }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Instantiate an <see cref="PagedViewList{T}"/>.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="curPage">The current page.</param>
        /// <param name="totalCount">The total count.</param>
        /// <param name="perPage">The count per page.</param>
        public PagedViewList(IReadOnlyList<T> content, int curPage, int totalCount, int perPage)
        {
            Content = content;
            CurrentPage = curPage;
            TotalCount = totalCount;
            TotalPage = (totalCount - 1) / perPage + 1;
            CountPerPage = perPage;
        }
    }

    /// <summary>
    /// Extension class for methods on <see cref="IPagedList{T}"/>.
    /// </summary>
    public static class PagedListExtensions
    {
        /// <summary>
        /// A paged <see cref="IReadOnlyList{T2}"/> view.
        /// </summary>
        /// <typeparam name="T1">The source type.</typeparam>
        /// <typeparam name="T2">The content type.</typeparam>
        private class PagedFakeList<T1, T2> : IPagedList<T2>
        {
            /// <inheritdoc />
            public T2 this[int index] => Selector.Invoke(Content[index]);

            /// <inheritdoc />
            public int Count => Content.Count;

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <inheritdoc />
            public IEnumerator<T2> GetEnumerator() => Content.Select(Selector).GetEnumerator();

            /// <inheritdoc />
            public int TotalCount { get; }

            /// <inheritdoc />
            public int TotalPage { get; }

            /// <inheritdoc />
            public int CountPerPage { get; }

            /// <inheritdoc />
            public int CurrentPage { get; }

            /// <summary>
            /// The internal list.
            /// </summary>
            private IReadOnlyList<T1> Content { get; set; }

            /// <summary>
            /// The internal selector.
            /// </summary>
            private Func<T1, T2> Selector { get; set; }

            /// <summary>
            /// Instantiate an <see cref="PagedFakeList{T1, T2}"/>.
            /// </summary>
            /// <param name="content">The content.</param>
            /// <param name="selector">The selector.</param>
            /// <param name="curPage">The current page.</param>
            /// <param name="totalCount">The total count.</param>
            /// <param name="perPage">The count per page.</param>
            public PagedFakeList(IReadOnlyList<T1> content, Func<T1, T2> selector, int curPage, int totalCount, int perPage)
            {
                Content = content;
                Selector = selector;
                CurrentPage = curPage;
                TotalCount = totalCount;
                TotalPage = (totalCount - 1) / perPage + 1;
                CountPerPage = perPage;
            }

            /// <summary>
            /// Instantiate an <see cref="PagedViewList{T}"/>.
            /// </summary>
            /// <param name="content">The content.</param>
            /// <param name="selector">The new content factory.</param>
            public PagedFakeList(IPagedList<T1> content, Func<T1, T2> selector)
            {
                Content = content;
                Selector = selector;
                CurrentPage = content.CurrentPage;
                TotalCount = content.TotalCount;
                TotalPage = (content.TotalCount - 1) / content.CountPerPage + 1;
                CountPerPage = content.CountPerPage;
            }
        }

        /// <summary>
        /// Cast this list with more details by selector.
        /// </summary>
        /// <typeparam name="T1">The source type.</typeparam>
        /// <typeparam name="T2">The target type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The new paged list.</returns>
        public static IPagedList<T2> As<T1, T2>(this IPagedList<T1> source, Func<T1, T2> selector)
        {
            return new PagedFakeList<T1, T2>(source, selector);
        }
    }
}
