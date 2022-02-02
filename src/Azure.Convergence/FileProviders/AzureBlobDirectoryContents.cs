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
        private readonly string[]? _allowedRanges;

        public AzureBlobDirectoryContents(
            BlobContainerClient client,
            string directoryPath,
            string localCachePath,
            bool allowAutoCache,
            string[]? allowedRanges)
        {
            _client = client;
            _directoryPath = directoryPath;
            _localCachePath = localCachePath;
            _allowAutoCache = allowAutoCache;
            _allowedRanges = allowedRanges;
        }

        public bool Exists => true;

        bool IFileInfo.IsDirectory => true;

        DateTimeOffset IFileInfo.LastModified => DateTimeOffset.UtcNow;

        long IFileInfo.Length => -1;

        string IFileInfo.Name => string.Empty;

        string? IFileInfo.PhysicalPath => null;

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            foreach (BlobHierarchyItem blobItem in
                _client.GetBlobsByHierarchy(
                    BlobTraits.Metadata,
                    BlobStates.None,
                    delimiter: "/",
                    prefix: _directoryPath.TrimStart('/')))
            {
                if (blobItem.IsPrefix && PermissionControl.IsAllowedDirectory(_allowedRanges, blobItem.Prefix))
                {
                    yield return new AzureBlobDirectoryContents(
                        _client,
                        blobItem.Prefix,
                        _localCachePath,
                        _allowAutoCache,
                        _allowedRanges);
                }
                else if (blobItem.IsBlob && PermissionControl.IsAllowedFile(_allowedRanges, blobItem.Blob.Name))
                {
                    StrongPath subpath = new(blobItem.Blob.Name);
                    yield return new AzureBlobInfo(
                        _client.GetBlobClient(subpath.GetLiteral()),
                        subpath.GetCachePath(_localCachePath, blobItem.Blob.Properties.ETag!.Value),
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

        public override string ToString()
        {
            return _client.Uri + "/" + _directoryPath;
        }
    }
}
