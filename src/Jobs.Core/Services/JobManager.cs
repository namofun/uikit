using Jobs.Entities;
using Jobs.Models;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jobs.Services
{
    /// <summary>
    /// The manager interface for jobs.
    /// </summary>
    public interface IJobManager
    {
        /// <summary>
        /// Gets the jobs with their children.
        /// </summary>
        /// <param name="ownerId">The user ID of owner.</param>
        /// <param name="page">The current page.</param>
        /// <param name="count">The count to display per page.</param>
        /// <returns>The list of job entries without their children.</returns>
        Task<IPagedList<JobEntry>> GetJobsAsync(int? ownerId, int page = 1, int count = 20);

        /// <summary>
        /// Gets children of the job with corresponding ID.
        /// </summary>
        /// <param name="id">The job ID.</param>
        /// <param name="ownerId">The user ID of owner.</param>
        /// <returns>The children of that job entry.</returns>
        Task<List<JobEntry>> GetChildrenAsync(Guid id, int? ownerId = null);

        /// <summary>
        /// Finds the job with corresponding ID.
        /// </summary>
        /// <param name="id">The job ID.</param>
        /// <param name="ownerId">The user ID of owner.</param>
        /// <returns>The job entry.</returns>
        Task<JobEntry?> FindJobAsync(Guid id, int? ownerId = null);

        /// <summary>
        /// Gets the logs with corresponding ID.
        /// </summary>
        /// <param name="jobEntry">The job entry.</param>
        /// <returns>The log file info.</returns>
        Task<string?> GetLogsAsync(Job jobEntry);

        /// <summary>
        /// Gets the download file with corresponding ID.
        /// </summary>
        /// <param name="jobEntry">The job entry.</param>
        /// <returns>The download file info.</returns>
        Task<IFileInfo?> GetDownloadAsync(Job jobEntry);
    }
}
