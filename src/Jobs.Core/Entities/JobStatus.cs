namespace SatelliteSite.Jobs.Entities
{
    /// <summary>
    /// The enum class for defining job status.
    /// </summary>
    public enum JobStatus
    {
        /// <summary>The job has not been started</summary>
        Pending,

        /// <summary>The job is still running</summary>
        Running,

        /// <summary>The job is done</summary>
        Finished,

        /// <summary>The job is failed</summary>
        Failed,
    }
}
