using Jobs.Entities;
using Jobs.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SatelliteSite.JobsModule.Services
{
    /// <summary>
    /// The file provider interface for jobs.
    /// </summary>
    public class PhysicalJobFileProvider : PhysicalBlobProvider, IJobFileProvider
    {
        private readonly Func<Job, string, string> _fileNameFormatter;

        /// <summary>
        /// Initialize the job file provider.
        /// </summary>
        /// <param name="path">The job path.</param>
        /// <param name="fileNameFormatter">The job file name formatter.</param>
        public PhysicalJobFileProvider(
            string path,
            Func<Job, string, string> fileNameFormatter = null)
            : base(path)
        {
            _fileNameFormatter = fileNameFormatter ?? ((job, type) => $"{job.JobId}/{type}");
        }

        /// <inheritdoc />
        public async Task<string> GetLogsAsync(Job entry)
        {
            IFileInfo file = GetFileInfo(_fileNameFormatter(entry, "log"));
            if (!file.Exists) return null;

            using Stream stream = file.CreateReadStream();
            using StreamReader reader = new(stream);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        public Task<IFileInfo> GetOutputAsync(Job entry)
        {
            return Task.FromResult(GetFileInfo(_fileNameFormatter(entry, "main")));
        }

        /// <inheritdoc />
        public Task SaveLogAsync(Job entry, string message)
        {
            return WriteStringAsync(_fileNameFormatter(entry, "log"), message);
        }

        /// <inheritdoc />
        public Task SaveOutputAsync(Job entry, Stream output)
        {
            return WriteStreamAsync(_fileNameFormatter(entry, "main"), output);
        }

        /// <inheritdoc />
        public Task SaveOutputAsync(Job entry, string output)
        {
            return WriteStringAsync(_fileNameFormatter(entry, "main"), output);
        }

        /// <inheritdoc />
        public Task SaveOutputAsync(Job entry, byte[] output)
        {
            return WriteBinaryAsync(_fileNameFormatter(entry, "main"), output);
        }
    }
}
