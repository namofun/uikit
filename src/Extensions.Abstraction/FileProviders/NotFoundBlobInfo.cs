using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders
{
    public class NotFoundBlobInfo : NotFoundFileInfo, IBlobInfo, IBlobFileInfo
    {
        public bool HasDirectLink => false;

        public NotFoundBlobInfo(string name) : base(name)
        {
        }

        public Task<Uri> CreateDirectLinkAsync(
            TimeSpan validPeriod,
            string? desiredDownloadName = null,
            string? desiredContentType = null,
            string? correlationId = null)
        {
            throw new FileNotFoundException("Blob does not exist.");
        }

        public Task<Stream> CreateReadStreamAsync(bool? cached)
        {
            return Task.FromResult(base.CreateReadStream());
        }
    }
}
