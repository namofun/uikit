using Jobs.Entities;
using Jobs.Models;
using System;
using System.Threading.Tasks;

namespace Jobs.Services
{
    /// <summary>
    /// The interface for scheduling jobs.
    /// </summary>
    public interface IJobScheduler
    {
        /// <summary>
        /// Schedules the job by description and persists into database.
        /// </summary>
        /// <param name="description">The job description.</param>
        /// <returns>The created job.</returns>
        Task<Job> ScheduleAsync(JobDescription description);

        /// <summary>
        /// Tries to dequeue a job.
        /// Note that this implementation do not have to be thread safe.
        /// </summary>
        /// <returns>The unfinished job. Null if no queued.</returns>
        Task<Job?> DequeueAsync();

        /// <summary>
        /// Marks the status of specified job.
        /// </summary>
        /// <param name="job">The job entity.</param>
        /// <param name="status">The job status.</param>
        /// <param name="completeTime">The job complete time.</param>
        /// <param name="prevStatus">The previous status to check. Null if check is not needed.</param>
        /// <returns>The task for marking.</returns>
        Task MarkAsync(Job job, JobStatus status, DateTimeOffset? completeTime = null, JobStatus? prevStatus = null);
    }
}
