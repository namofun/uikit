using System.Collections.Generic;

namespace Jobs.Models
{
    /// <summary>
    /// The model class for plans.
    /// </summary>
    public class JobDescription
    {
        /// <summary>
        /// The user ID of owner
        /// </summary>
        public int OwnerId { get; set; }

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

        /// <summary>
        /// The children jobs
        /// </summary>
        public List<JobDescription>? Children { get; set; }
    }
}
