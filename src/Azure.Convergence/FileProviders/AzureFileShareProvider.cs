using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders.AzureFileShare
{
    /// <summary>
    /// A read-write file provider over Azure Storage File Share.
    /// </summary>
    public class AzureFileShareProvider : IFileProvider
    {
        private readonly ShareDirectoryClient _rootDirectory;
        private readonly string[]? _allowedRanges;

        public AzureFileShareProvider(ShareDirectoryClient rootDirectory, string[]? allowedRanges = null)
        {
            _rootDirectory = rootDirectory;
            _allowedRanges = allowedRanges;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (!IsAllowedDirectory(_allowedRanges, subpath))
            {
                return NotFoundDirectoryContents.Singleton;
            }

            ShareDirectoryClient directory = _rootDirectory.GetSubdirectoryClient(subpath);
            return new AzureFileShareDirectoryContents(
                directory,
                directory.Exists().Value ? directory.GetProperties().Value.LastModified : null,
                GetAllowedSubdirectory(_allowedRanges, subpath));
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (!IsAllowedFile(_allowedRanges, subpath))
            {
                return new NotFoundFileInfo(Path.GetFileName(subpath));
            }

            ShareFileClient file = _rootDirectory.GetFileClient(subpath.TrimStart('/'));
            return new AzureFileShareFileInfo(file, file.Exists() ? file.GetProperties().Value : null);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }

        public async Task<IFileInfo> WriteStreamAsync(string subpath, Stream content)
        {
            if (!IsAllowedFile(_allowedRanges, subpath))
            {
                throw new InvalidOperationException("Permission denied when writing path.");
            }

            string tempFile = Path.GetTempFileName();
            try
            {
                using (FileStream fs = new(tempFile, FileMode.Create))
                {
                    await content.CopyToAsync(fs);
                }

                FileInfo tempFileInfo = new(tempFile);
                ShareFileClient file = _rootDirectory.GetFileClient(subpath);
                if (await file.ExistsAsync())
                {
                    await file.ForceCloseAllHandlesAsync();
                    await file.SetHttpHeadersAsync(newSize: tempFileInfo.Length);
                }
                else
                {
                    await file.CreateAsync(tempFileInfo.Length);
                }

                ShareFileUploadInfo uploadInfo;
                using (FileStream fs = new(tempFile, FileMode.Open))
                {
                    uploadInfo = await file.UploadAsync(content);
                }

                return new AzureFileShareFileInfo(file, (tempFileInfo.Length, uploadInfo.LastModified));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public static bool IsAllowedDirectory(string[]? allowedRanges, string path)
        {
            path = "/" + path.Trim('/', '\\').Replace('\\', '/') + "/";
            return allowedRanges == null
                || allowedRanges.Any(range => path.StartsWith(range) || range.StartsWith(path));
        }

        public static string[]? GetAllowedSubdirectory(string[]? allowedRanges, string path)
        {
            path = "/" + path.Trim('/', '\\').Replace('\\', '/') + "/";

            if (allowedRanges == null
                || allowedRanges.Any(range => path.StartsWith(range)))
                return null;

            return allowedRanges
                .Where(range => range.StartsWith("/" + path + "/"))
                .Select(range => range.Substring(0, path.Length - 1))
                .ToArray();
        }

        public static bool IsAllowedFile(string[]? allowedRanges, string path)
        {
            path = "/" + path.TrimStart('/', '\\').Replace('\\', '/');
            return allowedRanges == null
                || allowedRanges.Any(range =>
                    path == range
                    || (path.StartsWith(range) && range.EndsWith('/'))
                );
        }
    }
}
