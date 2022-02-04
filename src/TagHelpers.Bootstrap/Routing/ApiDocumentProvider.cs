namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    /// <summary>
    /// Provides the API documents.
    /// </summary>
    public interface IApiDocumentProvider
    {
        /// <summary>
        /// Provide the documents with title and specification JSON.
        /// </summary>
        /// <param name="name">The document name.</param>
        /// <param name="title">The document title.</param>
        /// <param name="spec">The document specification.</param>
        void GetDocument(string name, out string title, out string spec);
    }
}
