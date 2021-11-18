using Jobs.Entities;
using System.Collections.Generic;

namespace Jobs.Models
{
    /// <summary>
    /// The model class representing entries of job.
    /// </summary>
    public class JobEntry : Job
    {
        /// <summary>
        /// The children collection
        /// </summary>
        public ICollection<JobEntry>? Children { get; set; }
    }
}
