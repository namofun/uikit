using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders
{
    /// <summary>
    /// Extensions for reading <see cref="IFileInfo"/>.
    /// </summary>
    public static class FileRepositoryExtensions
    {
        /// <summary>
        /// Read the content of file as string.
        /// </summary>
        /// <param name="file">The file info.</param>
        /// <returns>The task for reading content. <c>Null</c> if the file does not exist.</returns>
        public static async Task<string?> ReadAsync(this IFileInfo file)
        {
            if (!file.Exists || file.IsDirectory)
                return null;
            using var fs = file.CreateReadStream();
            using var sw = new StreamReader(fs);
            return await sw.ReadToEndAsync();
        }

        /// <summary>
        /// Read the content of file as byte array.
        /// </summary>
        /// <param name="file">The file info.</param>
        /// <returns>The task for reading content. <c>Null</c> if the file does not exist.</returns>
        public static async Task<byte[]?> ReadBinaryAsync(this IFileInfo file)
        {
            if (!file.Exists || file.IsDirectory)
                return null;
            using var fs = file.CreateReadStream();
            var result = new byte[file.Length];
            await fs.ReadAsync(result, 0, result.Length);
            return result;
        }
    }
}
