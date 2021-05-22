using System;

namespace Jobs.Entities
{
    /// <summary>
    /// The entity class for a job.
    /// </summary>
    public class Job
    {
        /// <summary>
        /// The ID of job
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// The user ID of owner
        /// </summary>
        public int OwnerId { get; set; }

        /// <summary>
        /// The creation time
        /// </summary>
        public DateTimeOffset CreationTime { get; set; }

        /// <summary>
        /// The complete time
        /// </summary>
        public DateTimeOffset? CompleteTime { get; set; }

        /// <summary>
        /// The ID of parent job
        /// </summary>
        /// <remarks>Null if this is a root job.</remarks>
        public Guid? ParentJobId { get; set; }

        /// <summary>
        /// The status of current job
        /// </summary>
        public JobStatus Status { get; set; }

        /// <summary>
        /// The flag indicating whether this job is composite
        /// </summary>
        public bool Composite { get; set; }

        /// <summary>
        /// The suggested download file name
        /// </summary>
        public string? SuggestedFileName { get; set; }

        /// <summary>
        /// The type of the job
        /// </summary>
        public string? JobType { get; set; }

        /// <summary>
        /// The arguments of job
        /// </summary>
        public string? Arguments { get; set; }
    }
}
