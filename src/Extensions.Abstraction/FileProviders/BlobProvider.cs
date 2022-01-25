using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders
{
    /// <summary>
    /// An asynchronous read-write blob file provider abstraction.
    /// </summary>
    public interface IBlobProvider
    {
        /// <summary>
        /// Locate a file at the given path.
        /// </summary>
        /// <param name="subpath">Relative path that identifies the file.</param>
        /// <returns>The task for file information. Caller must check Exists property.</returns>
        Task<IBlobInfo> GetFileInfoAsync(string subpath);

        /// <summary>
        /// Write a file at the given path.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <param name="content">The string content to write</param>
        /// <param name="mime">File mime type to specify if downloading with direct link.</param>
        /// <returns>The task for file information.</returns>
        Task<IBlobInfo> WriteStringAsync(string subpath, string content, string mime = "application/octet-stream");

        /// <summary>
        /// Write a file at the given path.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <param name="content">The byte-array content to write</param>
        /// <param name="mime">File mime type to specify if downloading with direct link.</param>
        /// <returns>The task for file information.</returns>
        Task<IBlobInfo> WriteBinaryAsync(string subpath, byte[] content, string mime = "application/octet-stream");

        /// <summary>
        /// Write a file at the given path.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <param name="content">The stream content to write</param>
        /// <param name="mime">File mime type to specify if downloading with direct link.</param>
        /// <returns>The task for file information.</returns>
        Task<IBlobInfo> WriteStreamAsync(string subpath, Stream content, string mime = "application/octet-stream");

        /// <summary>
        /// Remove a file at the given path.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <returns>The task for removing files.</returns>
        Task<bool> RemoveFileAsync(string subpath);
    }
}
