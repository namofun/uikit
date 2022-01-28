using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders.AzureBlob
{
    public class AzureBlobInfo : IBlobInfo
    {
        public AzureBlobInfo(
            BlobClient blobClient,
            string localCachePath,
            long length,
            string name,
            DateTimeOffset lastModified)
        {
            Client = blobClient;
            Length = length;
            LastModified = lastModified;
            Name = Path.GetFileName(name);
            CachePath = localCachePath;
        }

        public BlobClient Client { get; }

        public bool Exists => true;

        public long Length { get; }

        public string? PhysicalPath => File.Exists(CachePath) ? CachePath : null;

        public bool HasDirectLink => Client.CanGenerateSasUri;

        public string Name { get; }

        public string CachePath { get; }

        public DateTimeOffset LastModified { get; }

        public Task<Uri> CreateDirectLinkAsync(TimeSpan validPeriod)
        {
            if (Client.CanGenerateSasUri)
            {
                return Task.FromResult(
                    Client.GenerateSasUri(
                        BlobSasPermissions.Read,
                        DateTimeOffset.Now + validPeriod));
            }
            else
            {
                throw new InvalidOperationException(
                    "Blob client doesn't support generate sas key.");
            }
        }

        public async Task<Stream> CreateReadStreamAsync(bool cached = false)
        {
            if (File.Exists(CachePath) && File.Exists(CachePath + ".date"))
            {
                return new FileStream(CachePath, FileMode.Open, FileAccess.Read, FileShare.Delete);
            }
            else if (cached)
            {
                string tempFile = Path.GetTempFileName();
                using Response resp = await Client.DownloadToAsync(tempFile);
                File.WriteAllText(CachePath + ".date", DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
                File.Move(tempFile, CachePath, overwrite: true);
                return new FileStream(CachePath, FileMode.Open, FileAccess.Read, FileShare.Delete);
            }
            else
            {
                return await Client.OpenReadAsync(allowBlobModifications: false);
            }
        }
    }
}
