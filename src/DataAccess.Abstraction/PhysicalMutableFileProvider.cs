using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders
{
    /// <inheritdoc />
    public class PhysicalMutableFileProvider : PhysicalFileProvider, IMutableFileProvider
    {
        /// <summary>
        /// Initializes a new instance of a PhysicalMutableFileProvider at the given root directory.
        /// </summary>
        /// <param name="root">The root directory. This should be an absolute path.</param>
        public PhysicalMutableFileProvider(string root) : base(root)
        {
        }


        /// <inheritdoc />
        public Task<bool> RemoveFileAsync(string subpath)
        {
            var fileInfo = GetFileInfo(subpath);
            if (fileInfo is NotFoundFileInfo || !fileInfo.Exists)
                return Task.FromResult(false);

            File.Delete(fileInfo.PhysicalPath);
            return Task.FromResult(true);
        }


        /// <inheritdoc />
        private void EnsureDirectoryExists(string subpath)
        {
            var path = Path.GetDirectoryName(subpath);
            Directory.CreateDirectory(path);
        }


        /// <inheritdoc />
        public async Task<IFileInfo> WriteBinaryAsync(string subpath, byte[] content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            var fileInfo = GetFileInfo(subpath);
            if (fileInfo is NotFoundFileInfo || fileInfo.IsDirectory)
                throw new InvalidOperationException();
            EnsureDirectoryExists(fileInfo.PhysicalPath);

            using FileStream stream = new FileStream(
                fileInfo.PhysicalPath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            await stream.WriteAsync(content, 0, content.Length);
            return fileInfo;
        }


        /// <inheritdoc />
        public async Task<IFileInfo> WriteStringAsync(string subpath, string content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            var fileInfo = GetFileInfo(subpath);
            if (fileInfo is NotFoundFileInfo || fileInfo.IsDirectory)
                throw new InvalidOperationException();
            EnsureDirectoryExists(fileInfo.PhysicalPath);

            using var stream = new FileStream(
                fileInfo.PhysicalPath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            using var streamWriter = new StreamWriter(stream, new UTF8Encoding(false));
            await streamWriter.WriteAsync(content);
            return fileInfo;
        }


        /// <inheritdoc />
        public async Task<IFileInfo> WriteStreamAsync(string subpath, Stream content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            var fileInfo = GetFileInfo(subpath);
            if (fileInfo is NotFoundFileInfo || fileInfo.IsDirectory)
                throw new InvalidOperationException();
            EnsureDirectoryExists(fileInfo.PhysicalPath);

            var fileInfo2 = new FileInfo(fileInfo.PhysicalPath);
            using var fs = fileInfo2.Open(FileMode.Create);
            await content.CopyToAsync(fs);
            return fileInfo;
        }


        /// <inheritdoc />
        public Task<IFileInfo> GetFileInfoAsync(string subpath)
        {
            return Task.FromResult(GetFileInfo(subpath));
        }


        /// <inheritdoc />
        public Task<IDirectoryContents> GetDirectoryContentsAsync(string subpath)
        {
            return Task.FromResult(GetDirectoryContents(subpath));
        }
    }
}
