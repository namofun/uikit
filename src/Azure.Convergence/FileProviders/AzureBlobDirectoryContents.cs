using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Extensions.FileProviders.AzureBlob
{
    public class AzureBlobDirectoryContents : IFileInfo, IDirectoryContents
    {
        private readonly BlobContainerClient _client;
        private readonly string _directoryPath;
        private readonly string _localCachePath;
        private readonly bool _allowAutoCache;

        public AzureBlobDirectoryContents(
            BlobContainerClient client,
            string directoryPath,
            string localCachePath,
            bool allowAutoCache)
        {
            _client = client;
            _directoryPath = directoryPath;
            _localCachePath = localCachePath;
            _allowAutoCache = allowAutoCache;
        }

        public bool Exists => true;

        bool IFileInfo.IsDirectory => true;

        DateTimeOffset IFileInfo.LastModified => DateTimeOffset.UtcNow;

        long IFileInfo.Length => -1;

        string IFileInfo.Name => _directoryPath == "/" ? "/" : Path.GetFileName(_directoryPath.TrimEnd('/'));

        string? IFileInfo.PhysicalPath => null;

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            foreach (BlobHierarchyItem blobItem in
                _client.GetBlobsByHierarchy(
                    BlobTraits.Metadata | BlobTraits.Tags,
                    BlobStates.None,
                    delimiter: "/",
                    prefix: _directoryPath))
            {
                if (blobItem.IsPrefix)
                {
                    yield return new AzureBlobDirectoryContents(
                        _client,
                        blobItem.Prefix,
                        _localCachePath,
                        _allowAutoCache);
                }
                else if (blobItem.IsBlob)
                {
                    if (!blobItem.Blob.Metadata.TryGetValue("LocalCacheGuid", out string? cacheGuid))
                    {
                        // This file cannot be accessed from this file provider.
                        continue;
                    }

                    StrongPath subpath = new(blobItem.Blob.Name);
                    yield return new AzureBlobInfo(
                        _client.GetBlobClient(subpath.GetLiteral()),
                        subpath.GetCachePath(_localCachePath, cacheGuid),
                        _allowAutoCache,
                        blobItem.Blob.Properties.ContentLength!.Value,
                        subpath.GetFileName(),
                        blobItem.Blob.Properties.LastModified!.Value);
                }
            }
        }

        Stream IFileInfo.CreateReadStream()
        {
            throw new InvalidOperationException("Cannot create read stream for a directory.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
