using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// The file provider for runtime razor compilation.
    /// </summary>
    public interface IRazorFileProvider : IFileProvider
    {
    }
}
