using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders.Physical
{
    public class PhysicalBlobInfo : PhysicalFileInfo, IBlobInfo
    {
        public bool HasDirectLink => false;

        public PhysicalBlobInfo(FileInfo info) : base(info)
        {
        }

        public Task<Uri> CreateDirectLinkAsync(TimeSpan validPeriod)
        {
            throw new NotSupportedException("Physical blob does not support direct link.");
        }

        public Task<Stream> CreateReadStreamAsync(bool cached = false)
        {
            return Task.FromResult(CreateReadStream());
        }
    }
}
