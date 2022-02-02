using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders.AzureBlob
{
    public class AzureBlobProvider : IFileProvider, IBlobProvider, IWritableFileProvider
    {
        private readonly string _localCachePath;
        private readonly BlobContainerClient _client;
        private readonly AccessTier? _defaultAccessTier;
        private readonly bool _allowAutoCache;
        private readonly IContentTypeProvider _contentTypeProvider;

        public AzureBlobProvider(
            BlobContainerClient client,
            string localFileCachePath,
            AccessTier? defaultAccessTier = default,
            bool allowAutoCache = false,
            IContentTypeProvider? contentTypeProvider = null)
        {
            _client = client;
            _localCachePath = localFileCachePath;
            _defaultAccessTier = defaultAccessTier;
            _allowAutoCache = allowAutoCache;
            _contentTypeProvider = contentTypeProvider ?? new FileExtensionContentTypeProvider();
        }

        private BlobClient GetBlobClient(StrongPath subpath)
        {
            return _client.GetBlobClient(subpath.GetLiteral());
        }

        private async ValueTask<IBlobFileInfo> InternalGetFileInfo(string _subpath, bool async)
        {
            StrongPath subpath = new(_subpath);
            BlobClient blob = this.GetBlobClient(subpath);
            if (!(async ? await blob.ExistsAsync().ConfigureAwait(false) : blob.Exists()))
            {
                return new NotFoundBlobInfo(subpath.GetFileName());
            }

            BlobProperties properties = async ? await blob.GetPropertiesAsync().ConfigureAwait(false) : blob.GetProperties();
            return new AzureBlobInfo(
                blob,
                subpath.GetCachePath(_localCachePath, properties.ETag),
                _allowAutoCache,
                properties.ContentLength,
                subpath.GetFileName(),
                properties.LastModified);
        }

        public async Task<IBlobInfo> GetFileInfoAsync(string subpath)
        {
            return await InternalGetFileInfo(subpath, async: true).ConfigureAwait(false);
        }

        public IFileInfo GetFileInfo(string _subpath)
        {
            ValueTask<IBlobFileInfo> task = InternalGetFileInfo(_subpath, async: false);
            System.Diagnostics.Debug.Assert(task.IsCompleted);
            return task.Result;
        }

        public async Task<bool> RemoveFileAsync(string subpath)
        {
            return await this.GetBlobClient(new(subpath))
                .DeleteIfExistsAsync()
                .ConfigureAwait(false);
        }

        public Task<IBlobInfo> WriteStringAsync(string subpath, string content, string mime = "application/octet-stream")
        {
            return WriteBinaryDataAsync(subpath, BinaryData.FromString(content), mime);
        }

        public Task<IBlobInfo> WriteBinaryAsync(string subpath, byte[] content, string mime = "application/octet-stream")
        {
            return WriteBinaryDataAsync(subpath, BinaryData.FromBytes(content), mime);
        }

        private async Task<IBlobInfo> WriteBinaryDataAsync(string _subpath, BinaryData content, string mime)
        {
            StrongPath subpath = new(_subpath);
            byte[] contentHash = MD5.HashData(content.ToMemory().Span);

            BlobClient blob = this.GetBlobClient(subpath);
            BlobContentInfo contentInfo = await blob
                .UploadAsync(content, CreateOptions(mime))
                .ConfigureAwait(false);

            if (!contentInfo.ContentHash.SequenceEqual(contentHash))
            {
                throw new InvalidDataException("File uploading corrupted, MD5 mismatch.");
            }

            return new AzureBlobInfo(
                blob,
                subpath.GetCachePath(_localCachePath, contentInfo.ETag),
                _allowAutoCache,
                content.ToMemory().Length,
                subpath.GetFileName(),
                contentInfo.LastModified);
        }

        private async Task<IBlobFileInfo> InternalWriteStreamAsync(string _subpath, Stream content, string mime = "application/octet-stream")
        {
            StrongPath subpath = new(_subpath);
            string tempFile = Path.GetTempFileName();

            try
            {
                byte[] contentHash;
                long fileLength;
                using (FileStream fileStream = new(tempFile, FileMode.Create))
                {
                    await content.CopyToAsync(fileStream).ConfigureAwait(false);
                    fileLength = fileStream.Position;
                    fileStream.Position = 0;

                    using HashAlgorithm md5 = MD5.Create();
                    contentHash = await md5.ComputeHashAsync(fileStream).ConfigureAwait(false);
                }

                if (!File.Exists(tempFile))
                {
                    throw new InvalidDataException("File caching corrupted, non existence.");
                }

                BlobClient blob = this.GetBlobClient(subpath);
                BlobContentInfo contentInfo = await blob
                    .UploadAsync(tempFile, CreateOptions(mime))
                    .ConfigureAwait(false);

                if (!contentInfo.ContentHash.SequenceEqual(contentHash))
                {
                    throw new InvalidDataException("File uploading corrupted, MD5 mismatch.");
                }

                return new AzureBlobInfo(
                    blob,
                    subpath.GetCachePath(_localCachePath, contentInfo.ETag),
                    _allowAutoCache,
                    fileLength,
                    subpath.GetFileName(),
                    contentInfo.LastModified);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public async Task<IBlobInfo> WriteStreamAsync(string subpath, Stream content, string mime = "application/octet-stream")
        {
            return await InternalWriteStreamAsync(subpath, content, mime).ConfigureAwait(false);
        }

        public async Task<IFileInfo> WriteStreamAsync(string subpath, Stream content)
        {
            if (!_contentTypeProvider.TryGetContentType(subpath, out string? mime))
                mime = "application/octet-stream";

            return await InternalWriteStreamAsync(subpath, content, mime).ConfigureAwait(false);
        }

        private BlobUploadOptions CreateOptions(string mime)
        {
            return new()
            {
                AccessTier = _defaultAccessTier,
                HttpHeaders = new BlobHttpHeaders()
                {
                    ContentType = mime,
                },
            };
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            subpath = subpath.Replace('\\', '/').Trim('/') + "/";

            StrongPath.EnsureValidPath_Length(subpath, includeZero: true);
            StrongPath.EnsureValidPath_Characters(subpath);

            if (!(_client
                .GetBlobsByHierarchy(BlobTraits.None, BlobStates.None, "/", subpath.TrimStart('/'))
                .AsPages(pageSizeHint: 1)
                .FirstOrDefault()?.Values.Any() ?? false))
            {
                return new NotFoundDirectoryContents();
            }

            return new AzureBlobDirectoryContents(_client, subpath, _localCachePath, _allowAutoCache);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }
    }
}
