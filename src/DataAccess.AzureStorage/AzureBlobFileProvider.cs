using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.FileProviders.AzureBlob;
using Microsoft.Extensions.FileProviders.Physical;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders
{
    public class AzureBlobFileProvider : IMutableFileProvider
    {
        private static readonly Regex _unusablePathChars = new("(" + string.Join('|', "\\/:?*\"<>|%".Select(ch => Regex.Escape(ch.ToString()))) + ")");
        private readonly BlobContainerClient _client;
        private readonly PhysicalMutableFileProvider _physicalCache;
        private readonly AccessTier? _defaultAccessTier;

        public AzureBlobFileProvider(
            BlobContainerClient client,
            PhysicalMutableFileProvider physicalCache,
            AccessTier? defaultAccessTier = default)
        {
            _client = client;
            _physicalCache = physicalCache;
            _defaultAccessTier = defaultAccessTier;
        }

        private static string GetNormalizedFileName(string subpath)
        {
            return _unusablePathChars.Replace(subpath, "__");
        }

        private static string GenerateLocalCacheGuid()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace('/', '@');
        }

        private BlobClient GetBlobClient(string subpath)
        {
            _ = subpath ?? throw new ArgumentNullException(nameof(subpath));

            subpath = subpath.TrimStart('/', '\\').Replace('\\', '/');
            return _client.GetBlobClient(subpath);
        }

        private IFileInfo GetLocalCacheFile(string subpath, string localCacheGuid)
        {
            _ = subpath ?? throw new ArgumentNullException(nameof(subpath));
            _ = localCacheGuid ?? throw new ArgumentNullException(nameof(localCacheGuid));

            subpath = subpath.TrimStart('/', '\\').Replace('\\', '/');
            return _physicalCache.GetFileInfo(GetNormalizedFileName(subpath) + "%" + localCacheGuid);
        }

        public async Task<IFileInfo> GetFileInfoAsync(string subpath)
        {
            BlobClient blob = this.GetBlobClient(subpath);
            if (!await blob.ExistsAsync().ConfigureAwait(false))
            {
                return new NotFoundFileInfo(Path.GetFileName(subpath));
            }

            BlobProperties properties = await blob.GetPropertiesAsync().ConfigureAwait(false);
            if (!properties.Metadata.TryGetValue("LocalCacheGuid", out string? cacheGuid))
            {
                throw new InvalidOperationException("Unknown blob uploaded.");
            }

            IFileInfo cachedFile = this.GetLocalCacheFile(subpath, cacheGuid);
            if (cachedFile is not PhysicalFileInfo)
            {
                throw new InvalidOperationException("Invalid file path format.");
            }

            if (!cachedFile.Exists || cachedFile.Length != properties.ContentLength)
            {
                await blob.DownloadToAsync(cachedFile.PhysicalPath).ConfigureAwait(false);
                cachedFile = this.GetLocalCacheFile(subpath, cacheGuid);
            }

            return new AzureBlobFileInfo(cachedFile, cachedFile.Length, subpath, properties.LastModified);
        }

        public async Task<bool> RemoveFileAsync(string subpath)
        {
            return await this.GetBlobClient(subpath).DeleteIfExistsAsync().ConfigureAwait(false);
        }

        public Task<IFileInfo> WriteStringAsync(string subpath, string content)
        {
            return WriteBinaryAsync(subpath, Encoding.UTF8.GetBytes(content));
        }

        public async Task<IFileInfo> WriteBinaryAsync(string subpath, byte[] content)
        {
            using MemoryStream memoryStream = new(content);
            return await WriteStreamAsync(subpath, memoryStream).ConfigureAwait(false);
        }

        public async Task<IFileInfo> WriteStreamAsync(string subpath, Stream content)
        {
            string storageTag = GenerateLocalCacheGuid();
            IFileInfo cachedFile = this.GetLocalCacheFile(subpath, storageTag);
            if (cachedFile is not PhysicalFileInfo)
            {
                throw new InvalidOperationException("Invalid file path format.");
            }

            byte[] contentHash;
            using (FileStream fileStream = new(cachedFile.PhysicalPath, FileMode.Create))
            {
                await content.CopyToAsync(fileStream).ConfigureAwait(false);
                fileStream.Position = 0;

                using HashAlgorithm md5 = MD5.Create();
                contentHash = await md5.ComputeHashAsync(fileStream).ConfigureAwait(false);
            }

            cachedFile = this.GetLocalCacheFile(subpath, storageTag);
            if (!cachedFile.Exists)
            {
                throw new InvalidDataException("File caching corrupted, non existence.");
            }

            BlobClient blob = this.GetBlobClient(subpath);
            BlobContentInfo contentInfo = await blob.UploadAsync(
                cachedFile.PhysicalPath,
                new BlobUploadOptions
                {
                    AccessTier = _defaultAccessTier,
                    Metadata = new Dictionary<string, string> { ["LocalCacheGuid"] = storageTag },
                })
                .ConfigureAwait(false);

            if (!contentInfo.ContentHash.SequenceEqual(contentHash))
            {
                throw new InvalidDataException("File uploading corrupted, MD5 mismatch.");
            }

            return new AzureBlobFileInfo(cachedFile, cachedFile.Length, subpath, contentInfo.LastModified);
        }
    }
}
