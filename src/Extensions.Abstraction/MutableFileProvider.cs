using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders
{
    /// <summary>
    /// An asynchronous read-write file provider abstraction.
    /// </summary>
    public interface IMutableFileProvider
    {
        /// <summary>
        /// Locate a file at the given path.
        /// </summary>
        /// <param name="subpath">Relative path that identifies the file.</param>
        /// <returns>The task for file information. Caller must check Exists property.</returns>
        Task<IFileInfo> GetFileInfoAsync(string subpath);

        /// <summary>
        /// Enumerate a directory at the given path, if any.
        /// </summary>
        /// <param name="subpath">Relative path that identifies the directory.</param>
        /// <returns>The task for contents of the directory.</returns>
        Task<IDirectoryContents> GetDirectoryContentsAsync(string subpath);

        /// <summary>
        /// Write a file at the given path.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <param name="content">The string content to write</param>
        /// <returns>The task for file information.</returns>
        Task<IFileInfo> WriteStringAsync(string subpath, string content);

        /// <summary>
        /// Write a file at the given path.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <param name="content">The byte-array content to write</param>
        /// <returns>The task for file information.</returns>
        Task<IFileInfo> WriteBinaryAsync(string subpath, byte[] content);

        /// <summary>
        /// Write a file at the given path.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <param name="content">The stream content to write</param>
        /// <returns>The task for file information.</returns>
        Task<IFileInfo> WriteStreamAsync(string subpath, Stream content);

        /// <summary>
        /// Remove a file at the given path.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <returns>The task for removing files.</returns>
        Task<bool> RemoveFileAsync(string subpath);
    }
}
