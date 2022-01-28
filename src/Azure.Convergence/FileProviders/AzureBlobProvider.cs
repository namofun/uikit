using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders.AzureBlob
{
    public class AzureBlobProvider : IBlobProvider
    {
        private readonly string _localCachePath;
        private readonly BlobContainerClient _client;
        private readonly AccessTier? _defaultAccessTier;

        public AzureBlobProvider(
            BlobContainerClient client,
            string localFileCachePath,
            AccessTier? defaultAccessTier = default)
        {
            _client = client;
            _localCachePath = localFileCachePath;
            _defaultAccessTier = defaultAccessTier;
        }

        private static string GenerateLocalCacheGuid()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace('/', '@');
        }

        private BlobClient GetBlobClient(StrongPath subpath)
        {
            return _client.GetBlobClient(subpath.GetLiteral());
        }

        private string GetLocalCacheFilePath(StrongPath subpath, string localCacheGuid)
        {
            _ = localCacheGuid ?? throw new ArgumentNullException(nameof(localCacheGuid));
            return Path.Combine(_localCachePath, subpath.Normalize() + "%" + localCacheGuid);
        }

        public async Task<IBlobInfo> GetFileInfoAsync(string _subpath)
        {
            StrongPath subpath = new(_subpath);
            BlobClient blob = this.GetBlobClient(subpath);
            if (!await blob.ExistsAsync().ConfigureAwait(false))
            {
                return new NotFoundBlobInfo(subpath.GetFileName());
            }

            BlobProperties properties = await blob.GetPropertiesAsync().ConfigureAwait(false);
            if (!properties.Metadata.TryGetValue("LocalCacheGuid", out string? cacheGuid))
            {
                throw new InvalidOperationException("Unknown blob uploaded.");
            }

            return new AzureBlobInfo(
                blob,
                GetLocalCacheFilePath(subpath, cacheGuid),
                properties.ContentLength,
                subpath.GetFileName(),
                properties.LastModified);
        }

        public async Task<bool> RemoveFileAsync(string subpath)
        {
            return await this.GetBlobClient(new(subpath))
                .DeleteIfExistsAsync()
                .ConfigureAwait(false);
        }

        public Task<IBlobInfo> WriteStringAsync(string subpath, string content, string mime)
        {
            return WriteBinaryDataAsync(subpath, BinaryData.FromString(content), mime);
        }

        public Task<IBlobInfo> WriteBinaryAsync(string subpath, byte[] content, string mime)
        {
            return WriteBinaryDataAsync(subpath, BinaryData.FromBytes(content), mime);
        }

        private async Task<IBlobInfo> WriteBinaryDataAsync(string _subpath, BinaryData content, string mime)
        {
            StrongPath subpath = new(_subpath);
            string storageTag = GenerateLocalCacheGuid();
            byte[] contentHash = MD5.HashData(content.ToMemory().Span);

            BlobClient blob = this.GetBlobClient(subpath);
            BlobContentInfo contentInfo = await blob
                .UploadAsync(content, CreateOptions(storageTag, mime))
                .ConfigureAwait(false);

            if (!contentInfo.ContentHash.SequenceEqual(contentHash))
            {
                throw new InvalidDataException("File uploading corrupted, MD5 mismatch.");
            }

            return new AzureBlobInfo(
                blob,
                GetLocalCacheFilePath(subpath, storageTag),
                content.ToMemory().Length,
                subpath.GetFileName(),
                contentInfo.LastModified);
        }

        public async Task<IBlobInfo> WriteStreamAsync(string _subpath, Stream content, string mime)
        {
            StrongPath subpath = new(_subpath);
            string storageTag = GenerateLocalCacheGuid();
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
                    .UploadAsync(tempFile, CreateOptions(storageTag, mime))
                    .ConfigureAwait(false);

                if (!contentInfo.ContentHash.SequenceEqual(contentHash))
                {
                    throw new InvalidDataException("File uploading corrupted, MD5 mismatch.");
                }

                return new AzureBlobInfo(
                    blob,
                    GetLocalCacheFilePath(subpath, storageTag),
                    fileLength,
                    subpath.GetFileName(),
                    contentInfo.LastModified);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        private BlobUploadOptions CreateOptions(string storageTag, string mime)
        {
            return new()
            {
                AccessTier = _defaultAccessTier,
                HttpHeaders = new BlobHttpHeaders()
                {
                    ContentType = mime,
                },
                Metadata = new Dictionary<string, string>()
                {
                    ["LocalCacheGuid"] = storageTag,
                },
            };
        }

        private readonly struct StrongPath
        {
            private const string InvalidCharacters = ":?*\"'`#$&<>|%";
            private static readonly Regex _unusablePathChars =
                new("(" + string.Join('|', (InvalidCharacters + "/\\").Select(ch => Regex.Escape(ch.ToString()))) + ")");

            private readonly string _path;

            public StrongPath(string subpath)
            {
                subpath = subpath.TrimStart('/', '\\').Replace('\\', '/');
                if (subpath.Length == 0)
                {
                    throw new ArgumentException(
                        "Path cannot be empty.",
                        nameof(subpath));
                }

                if (subpath.Length > 100)
                {
                    throw new ArgumentException(
                        "Path cannot be longer than 100.",
                        nameof(subpath));
                }

                if (subpath.EndsWith("/"))
                {
                    throw new ArgumentException(
                        "Path cannot be a directory.",
                        nameof(subpath));
                }

                if (subpath.Contains("//"))
                {
                    throw new ArgumentException(
                        "Path cannot include consecutive '/'.",
                        nameof(subpath));
                }

                if (InvalidCharacters.Any(subpath.Contains))
                {
                    throw new ArgumentException(
                        "Path cannot include any characters in  '/'.",
                        nameof(subpath));
                }

                _path = subpath;
            }

            public string GetFileName()
            {
                return System.IO.Path.GetFileName(_path);
            }

            public string Normalize()
            {
                return _unusablePathChars.Replace(_path, "__");
            }

            public string GetLiteral()
            {
                return _path;
            }
        }
    }
}
