using Microsoft.AspNetCore.Hosting;

namespace Microsoft.Extensions.FileProviders
{
    internal class WwwrootFileProvider : PhysicalMutableFileProvider, IWwwrootFileProvider
    {
        public WwwrootFileProvider(IWebHostEnvironment environment) : base(environment.WebRootPath)
        {
        }
    }
}
