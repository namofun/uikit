using System;

namespace SatelliteSite.Jobs.Entities
{
    /// <summary>
    /// The entity class for a job.
    /// </summary>
    public class Job
    {
        /// <summary>
        /// The ID of job
        /// </summary>
        public int JobId { get; set; }

        /// <summary>
        /// The ID of parent job
        /// </summary>
        /// <remarks>Null if this is a root job.</remarks>
        public int? ParentJobId { get; set; }

        /// <summary>
        /// The storage unique ID
        /// </summary>
        /// <remarks>This field is set only when this job is concret.</remarks>
        public Guid? StorageId { get; set; }

        /// <summary>
        /// The status of current job
        /// </summary>
        public JobStatus Status { get; set; }

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
