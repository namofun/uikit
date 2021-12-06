using System;
using System.IO;

namespace Microsoft.Extensions.FileProviders.AzureBlob
{
    public class AzureBlobFileInfo : IFileInfo
    {
        private readonly IFileInfo cachedFileInfo;

        public AzureBlobFileInfo(
            IFileInfo cachedFileInfo,
            long length,
            string name,
            DateTimeOffset lastModified)
        {
            this.cachedFileInfo = cachedFileInfo;
            this.LastModified = lastModified;
            this.Length = length;
            this.Name = Path.GetFileName(name);
        }

        public bool Exists => true;

        public long Length { get; }

        public string? PhysicalPath => cachedFileInfo.PhysicalPath;

        public string Name { get; }

        public DateTimeOffset LastModified { get; }

        public bool IsDirectory => false;

        public Stream CreateReadStream() => cachedFileInfo.CreateReadStream();
    }
}
