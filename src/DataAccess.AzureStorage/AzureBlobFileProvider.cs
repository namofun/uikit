using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.FileProviders.AzureBlob;
using Microsoft.Extensions.FileProviders.Physical;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders
{
    public class AzureBlobFileProvider : IMutableFileProvider
    {
        private readonly BlobContainerClient _client;
        private readonly PhysicalMutableFileProvider _physicalCache;
        private readonly AccessTier _defaultAccessTier;

        public AzureBlobFileProvider(
            BlobContainerClient client,
            PhysicalMutableFileProvider physicalCache,
            AccessTier defaultAccessTier)
        {
            _client = client;
            _physicalCache = physicalCache;
            _defaultAccessTier = defaultAccessTier;
        }

        public Task<IDirectoryContents> GetDirectoryContentsAsync(string subpath)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IFileInfo> GetFileInfoAsync(string subpath)
        {
            BlobClient blob = _client.GetBlobClient(subpath);
            if (!await blob.ExistsAsync())
            {
                return new NotFoundFileInfo(Path.GetFileName(subpath));
            }

            BlobProperties properties = await blob.GetPropertiesAsync();
            if (!properties.Metadata.ContainsKey("LocalCacheGuid"))
            {
                throw new InvalidOperationException("Unknown blob uploaded.");
            }

            IFileInfo cachedFile = _physicalCache.GetFileInfo(subpath + "%" + properties.Metadata["LocalCacheGuid"]);
            if (cachedFile is not PhysicalFileInfo)
            {
                throw new InvalidOperationException("Invalid file path format.");
            }

            if (!cachedFile.Exists || cachedFile.Length != properties.ContentLength)
            {
                await blob.DownloadToAsync(cachedFile.PhysicalPath);
            }

            return new AzureBlobFileInfo(cachedFile, cachedFile.Length, subpath, properties.LastModified);
        }

        public async Task<bool> RemoveFileAsync(string subpath)
        {
            return await _client.DeleteBlobIfExistsAsync(subpath).ConfigureAwait(false);
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
            string storageTag = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace('/', '@');
            IFileInfo cachedFile = _physicalCache.GetFileInfo(subpath + "%" + storageTag);
            if (cachedFile is not PhysicalFileInfo)
            {
                throw new InvalidOperationException("Invalid file path format.");
            }

            using (FileStream fs = new(cachedFile.PhysicalPath, FileMode.Create))
            {
                await content.CopyToAsync(fs).ConfigureAwait(false);
            }

            if (!cachedFile.Exists)
            {
                throw new NotImplementedException();
            }

            BlobClient blob = _client.GetBlobClient(subpath);
            BlobContentInfo contentInfo = await blob.UploadAsync(
                cachedFile.PhysicalPath,
                new BlobUploadOptions
                {
                    AccessTier = _defaultAccessTier,
                    Metadata = { ["LocalCacheGuid"] = storageTag },
                })
                .ConfigureAwait(false);

            return new AzureBlobFileInfo(cachedFile, cachedFile.Length, subpath, contentInfo.LastModified);
        }
    }
}
