using Jobs.Entities;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Threading.Tasks;

namespace Jobs.Services
{
    /// <summary>
    /// The file provider interface for jobs.
    /// </summary>
    public interface IJobFileProvider
    {
        /// <summary>
        /// Saves the output of job.
        /// </summary>
        /// <param name="jobEntry">The job entry.</param>
        /// <param name="output">The output stream.</param>
        /// <returns>The task for saving output stream.</returns>
        Task SaveOutputAsync(Job jobEntry, Stream output);

        /// <summary>
        /// Saves the output of job.
        /// </summary>
        /// <param name="jobEntry">The job entry.</param>
        /// <param name="output">The output string.</param>
        /// <returns>The task for saving output string.</returns>
        Task SaveOutputAsync(Job jobEntry, string output);

        /// <summary>
        /// Saves the output of job.
        /// </summary>
        /// <param name="jobEntry">The job entry.</param>
        /// <param name="output">The output bytes.</param>
        /// <returns>The task for saving output bytes.</returns>
        Task SaveOutputAsync(Job jobEntry, byte[] output);

        /// <summary>
        /// Saves the log of job.
        /// </summary>
        /// <param name="jobEntry">The job entry.</param>
        /// <param name="message">The log message.</param>
        /// <returns>The task for saving log message.</returns>
        Task SaveLogAsync(Job jobEntry, string message);

        /// <summary>
        /// Gets the log of job.
        /// </summary>
        /// <param name="jobEntry">The job entry.</param>
        /// <returns>The task for reading log message.</returns>
        Task<string?> GetLogsAsync(Job jobEntry);

        /// <summary>
        /// Gets the output of job.
        /// </summary>
        /// <param name="jobEntry">The job entry.</param>
        /// <returns>The task for reading output stream.</returns>
        Task<IFileInfo?> GetOutputAsync(Job jobEntry);
    }
}
