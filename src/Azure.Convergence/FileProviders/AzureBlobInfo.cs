using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders.AzureBlob
{
    public class AzureBlobInfo : IBlobInfo, IFileInfo, IBlobFileInfo
    {
        public AzureBlobInfo(
            BlobClient blobClient,
            string localCachePath,
            bool allowAutoCahce,
            long length,
            string name,
            DateTimeOffset lastModified)
        {
            Client = blobClient;
            Length = length;
            LastModified = lastModified;
            Name = Path.GetFileName(name);
            CachePath = localCachePath;
            AutoCache = allowAutoCahce;
        }

        public BlobClient Client { get; }

        public bool Exists => true;

        public long Length { get; }

        public string? PhysicalPath => File.Exists(CachePath) ? CachePath : null;

        public bool HasDirectLink => Client.CanGenerateSasUri;

        public string Name { get; }

        public string CachePath { get; }

        public bool AutoCache { get; }

        public DateTimeOffset LastModified { get; }

        public bool IsDirectory => false;

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

        private async ValueTask<Stream> InternalCreateReadStream(bool autoCache, bool async)
        {
            if (autoCache)
            {
                if (!File.Exists(CachePath))
                {
                    string tempFile = Path.GetTempFileName();
                    using Response resp = async
                        ? await Client.DownloadToAsync(tempFile).ConfigureAwait(false)
                        : Client.DownloadTo(tempFile);

                    File.Move(tempFile, CachePath, overwrite: true);
                }

                return new FileStream(CachePath, FileMode.Open, FileAccess.Read, FileShare.Delete);
            }
            else
            {
                return async
                    ? await Client.OpenReadAsync(allowBlobModifications: false).ConfigureAwait(false)
                    : Client.OpenRead(allowBlobModifications: false);
            }
        }

        public Stream CreateReadStream()
        {
            ValueTask<Stream> result = InternalCreateReadStream(AutoCache, async: false);
            System.Diagnostics.Debug.Assert(result.IsCompleted);
            return result.Result;
        }

        public async Task<Stream> CreateReadStreamAsync(bool? cached)
        {
            return await InternalCreateReadStream(cached ?? AutoCache, async: true).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return Client.Uri.ToString();
        }
    }
}
