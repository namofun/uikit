namespace Jobs.Entities
{
    /// <summary>
    /// The enum class for defining job status.
    /// </summary>
    public enum JobStatus
    {
        /// <summary>The job status is unknown</summary>
        Unknown,

        /// <summary>The job is composite</summary>
        Composite,

        /// <summary>The job has not been started</summary>
        Pending,

        /// <summary>The job is still running</summary>
        Running,

        /// <summary>The job is done</summary>
        Finished,

        /// <summary>The job is failed</summary>
        Failed,

        /// <summary>The job is cancelled</summary>
        Cancelled,
    }
}
