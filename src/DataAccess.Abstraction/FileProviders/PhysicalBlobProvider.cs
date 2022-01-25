using Microsoft.Extensions.FileProviders.Physical;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders
{
    /// <inheritdoc />
    public class PhysicalBlobProvider : PhysicalFileProvider, IBlobProvider
    {
        /// <summary>
        /// Initializes a new instance of a PhysicalMutableFileProvider at the given root directory.
        /// </summary>
        /// <param name="root">The root directory. This should be an absolute path.</param>
        public PhysicalBlobProvider(string root) : base(root)
        {
        }


        /// <inheritdoc />
        public Task<bool> RemoveFileAsync(string subpath)
        {
            IFileInfo fileInfo = GetFileInfo(subpath);
            if (fileInfo is NotFoundFileInfo || !fileInfo.Exists)
            {
                return Task.FromResult(false);
            }

            File.Delete(fileInfo.PhysicalPath);
            return Task.FromResult(true);
        }


        /// <inheritdoc />
        private static void EnsureDirectoryExists(string subpath)
        {
            string path = Path.GetDirectoryName(subpath)!;
            Directory.CreateDirectory(path);
        }


        /// <inheritdoc />
        public async Task<IBlobInfo> WriteBinaryAsync(string subpath, byte[] content, string mime = "application/octet-stream")
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            IFileInfo fileInfo = GetFileInfo(subpath);
            if (fileInfo is NotFoundFileInfo || fileInfo.IsDirectory)
            {
                throw new InvalidOperationException();
            }

            EnsureDirectoryExists(fileInfo.PhysicalPath);

            using FileStream stream = new(
                fileInfo.PhysicalPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            await stream.WriteAsync(content.AsMemory());
            return Convert(fileInfo);
        }


        /// <inheritdoc />
        public async Task<IBlobInfo> WriteStringAsync(string subpath, string content, string mime = "application/octet-stream")
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            IFileInfo fileInfo = GetFileInfo(subpath);
            if (fileInfo is NotFoundFileInfo || fileInfo.IsDirectory)
            {
                throw new InvalidOperationException();
            }

            EnsureDirectoryExists(fileInfo.PhysicalPath);

            using FileStream stream = new(
                fileInfo.PhysicalPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            using StreamWriter streamWriter = new(stream, new UTF8Encoding(false));
            await streamWriter.WriteAsync(content);
            return Convert(fileInfo);
        }


        /// <inheritdoc />
        public async Task<IBlobInfo> WriteStreamAsync(string subpath, Stream content, string mime = "application/octet-stream")
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            IFileInfo fileInfo = GetFileInfo(subpath);
            if (fileInfo is NotFoundFileInfo || fileInfo.IsDirectory)
            {
                throw new InvalidOperationException();
            }

            EnsureDirectoryExists(fileInfo.PhysicalPath);

            FileInfo fileInfo2 = new(fileInfo.PhysicalPath);
            using FileStream fs = fileInfo2.Open(FileMode.Create);
            await content.CopyToAsync(fs);
            return Convert(fileInfo);
        }


        /// <inheritdoc />
        public Task<IBlobInfo> GetFileInfoAsync(string subpath)
        {
            return Task.FromResult(Convert(GetFileInfo(subpath)));
        }


        /// <summary>
        /// Converts the file info to blob info.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        /// <returns>The blob info.</returns>
        /// <exception cref="NotImplementedException">The file info is not supported.</exception>
        private static IBlobInfo Convert(IFileInfo fileInfo)
        {
            return fileInfo switch
            {
                NotFoundFileInfo nf => new NotFoundBlobInfo(nf.Name),
                PhysicalFileInfo pf => new PhysicalBlobInfo(new FileInfo(pf.PhysicalPath)),
                _ => throw new NotImplementedException("Did not implement for " + fileInfo.GetType().Name),
            };
        }
    }
}
