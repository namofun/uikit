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
        /// <returns>Instance of document.</returns>
        IApiDocument GetDocument(string name);
    }

    /// <summary>
    /// Interface for the API document.
    /// </summary>
    public interface IApiDocument
    {
        /// <summary>
        /// The document name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The document title
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the API specification.
        /// </summary>
        /// <param name="type">The specification type.</param>
        /// <returns>The specification content.</returns>
        string GetSpecification(ApiSpecificationType type);
    }

    /// <summary>
    /// Enum for API specification type.
    /// </summary>
    public enum ApiSpecificationType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Swagger V2 document JSON
        /// </summary>
        SwaggerV2Json,

        /// <summary>
        /// Swagger V3 document JSON
        /// </summary>
        SwaggerV3Json,

        /// <summary>
        /// Swagger V2 document YAML
        /// </summary>
        SwaggerV2Yaml,

        /// <summary>
        /// Swagger V3 document YAML
        /// </summary>
        SwaggerV3Yaml,
    }
}
