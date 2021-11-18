using Microsoft.Extensions.FileProviders;

namespace Jobs.Services
{
    /// <summary>
    /// The file provider interface for jobs.
    /// </summary>
    public interface IJobFileProvider : IMutableFileProvider
    {
    }
}
