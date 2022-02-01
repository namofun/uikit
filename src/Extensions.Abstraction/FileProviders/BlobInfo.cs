using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders
{
    /// <summary>
    /// Represents a file in the file provider.
    /// </summary>
    public interface IBlobInfo
    {
        /// <summary>
        /// True if resource exists in the underlying storage system.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// The length of the file in bytes, or -1 for a directory or non-existing files.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// The path to the file, including the file name.
        /// Return null if the file is not directly accessible.
        /// </summary>
        string? PhysicalPath { get; }

        /// <summary>
        /// True if resource can be accessed by direct link.
        /// </summary>
        bool HasDirectLink { get; }

        /// <summary>
        /// The name of the file or directory, not including any path.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// When the file was last modified
        /// </summary>
        DateTimeOffset LastModified { get; }

        /// <summary>
        /// Creates a direct link for two hour access.
        /// </summary>
        /// <param name="validPeriod">The valid time period.</param>
        /// <returns>The link to access file without permission.</returns>
        Task<Uri> CreateDirectLinkAsync(TimeSpan validPeriod);

        /// <summary>
        /// Return file contents as readonly stream. Caller should dispose stream when complete.
        /// </summary>
        /// <param name="cached">Whether the file should be cached for next access if this is a remote blob.</param>
        /// <returns>The file stream</returns>
        Task<Stream> CreateReadStreamAsync(bool cached = false);
    }

    /// <summary>
    /// Combination interface for <see cref="IBlobInfo"/> and <see cref="IFileInfo"/>.
    /// </summary>
    internal interface IBlobFileInfo : IBlobInfo, IFileInfo
    {
    }
}
