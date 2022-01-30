using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders
{
    /// <summary>
    /// A read-write file provider for wwwroot contents.
    /// </summary>
    public interface IWwwrootFileProvider : IFileProvider
    {
        /// <summary>
        /// Write a file at the given path.
        /// </summary>
        /// <param name="subpath">Relative path that identifies the file.</param>
        /// <param name="content">The stream content to write.</param>
        /// <returns>The file information.</returns>
        Task<IFileInfo> WriteStreamAsync(string subpath, Stream content);
    }
}
