﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders
{
    public class NotFoundBlobInfo : NotFoundFileInfo, IBlobInfo
    {
        public bool HasDirectLink => false;

        public NotFoundBlobInfo(string name) : base(name)
        {
        }

        public Task<Uri> CreateDirectLinkAsync(TimeSpan validPeriod)
        {
            throw new FileNotFoundException("Blob does not exist.");
        }

        public Task<Stream> CreateReadStreamAsync(bool cached = false)
        {
            return Task.FromResult(base.CreateReadStream());
        }
    }
}