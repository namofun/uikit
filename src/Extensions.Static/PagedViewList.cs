namespace System.Collections.Generic
{
    /// <summary>
    /// A paged <see cref="IReadOnlyList{T}"/> view.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    public class PagedViewList<T> : IReadOnlyList<T>
    {
        /// <inheritdoc />
        public T this[int index] => Content[index];

        /// <inheritdoc />
        public int Count => Content.Count;

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => Content.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => Content.GetEnumerator();

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
        private List<T> Content { get; set; }

        /// <summary>
        /// Instantiate an <see cref="PagedViewList{T}"/>.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="curPage">The current page.</param>
        /// <param name="totalCount">The total count.</param>
        /// <param name="perPage">The count per page.</param>
        public PagedViewList(List<T> content, int curPage, int totalCount, int perPage)
        {
            Content = content;
            CurrentPage = curPage;
            TotalCount = totalCount;
            TotalPage = (content.Count - 1) / perPage + 1;
            CountPerPage = perPage;
        }
    }
}
