using Azure.Storage.Files.Shares;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Microsoft.Extensions.FileProviders.AzureFileShare
{
    [Obsolete("Did not test through")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AzureFileShareDirectoryContents : IFileInfo, IDirectoryContents
    {
        private readonly ShareDirectoryClient _directory;
        private readonly DateTimeOffset? _lastModified;
        private readonly string[]? _allowedRanges;

        public AzureFileShareDirectoryContents(
            ShareDirectoryClient directory,
            DateTimeOffset? lastModified,
            string[]? allowedRanges)
        {
            _directory = directory;
            _lastModified = lastModified;
            _allowedRanges = allowedRanges;
        }

        public bool Exists => _lastModified.HasValue;

        public bool IsDirectory => true;

        public DateTimeOffset LastModified => _lastModified ?? DateTimeOffset.MinValue;

        public long Length => -1;

        public string Name => _directory.Name;

        public string PhysicalPath => _directory.Uri.AbsoluteUri;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Stream CreateReadStream()
        {
            throw new InvalidOperationException("Cannot create read stream for a directory.");
        }

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            foreach (var item in _directory.GetFilesAndDirectories())
            {
                if (item.IsDirectory && PermissionControl.IsAllowedDirectory(_allowedRanges, item.Name))
                {
                    yield return new AzureFileShareDirectoryContents(
                        _directory.GetSubdirectoryClient(item.Name),
                        item.Properties.LastModified!.Value,
                        PermissionControl.GetAllowedSubdirectory(_allowedRanges, item.Name));
                }
                else if (PermissionControl.IsAllowedFile(_allowedRanges, item.Name))
                {
                    yield return new AzureFileShareFileInfo(
                        _directory.GetFileClient(item.Name),
                        (item.FileSize!.Value, item.Properties.LastModified!.Value));
                }
            }
        }
    }
}
