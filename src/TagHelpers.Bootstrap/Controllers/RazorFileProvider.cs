using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Razor
{
    /// <summary>
    /// The file provider for runtime razor compilation.
    /// </summary>
    public interface IRazorFileProvider : IFileProvider
    {
        /// <summary>
        /// Adds the physical provider to this peer file provider.
        /// </summary>
        /// <param name="fileProvider">The physical file provider.</param>
        /// <returns>The peer file provider.</returns>
        IRazorFileProvider Append(PhysicalFileProvider fileProvider);

        /// <summary>
        /// Gets the sub file provider.
        /// </summary>
        /// <param name="name">The folder name.</param>
        /// <returns>The file provider.</returns>
        IRazorFileProvider this[string name] { get; }

        /// <summary>
        /// Injects the logger into file provider.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        void InjectLogger(ILogger? logger);
    }
}
