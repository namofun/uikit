using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System;
using System.IO;

namespace Microsoft.Extensions.FileProviders.AzureFileShare
{
    public class AzureFileShareFileInfo : IFileInfo
    {
        private readonly ShareFileClient _fileClient;
        private readonly (long ContentLength, DateTimeOffset LastModified)? _properties;

        public AzureFileShareFileInfo(ShareFileClient file, (long ContentLength, DateTimeOffset LastModified)? properties = null)
        {
            _fileClient = file;
            _properties = properties;
        }

        public AzureFileShareFileInfo(ShareFileClient file, ShareFileProperties? properties = null)
        {
            _fileClient = file;
            _properties = properties == null ? null : (properties.ContentLength, properties.LastModified);
        }

        public bool Exists => _properties.HasValue;

        public bool IsDirectory => false;

        public DateTimeOffset LastModified => _properties?.LastModified ?? DateTimeOffset.MinValue;

        public long Length => _properties?.ContentLength ?? -1;

        public string Name => Path.GetFileName(_fileClient.Name);

        public string PhysicalPath => _fileClient.Uri.AbsoluteUri;

        public Stream CreateReadStream()
        {
            if (_properties.HasValue)
            {
                return _fileClient.OpenRead(allowfileModifications: false);
            }
            else
            {
                throw new FileNotFoundException();
            }
        }
    }
}
