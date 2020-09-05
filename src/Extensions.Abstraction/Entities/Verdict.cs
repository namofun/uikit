namespace SatelliteSite.Entities
{
    /// <summary>
    /// The verdicts used in results.
    /// </summary>
    public enum Verdict
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Time Limit Exceeded
        /// </summary>
        TimeLimitExceeded = 1,

        /// <summary>
        /// Memory Limit Exceeded
        /// </summary>
        MemoryLimitExceeded = 2,

        /// <summary>
        /// Runtime Error
        /// </summary>
        RuntimeError = 3,

        /// <summary>
        /// Output Limit Exceeded
        /// </summary>
        OutputLimitExceeded = 4,

        /// <summary>
        /// Wrong Answer
        /// </summary>
        WrongAnswer = 5,

        /// <summary>
        /// Compile Error
        /// </summary>
        CompileError = 6,

        /// <summary>
        /// Presentation Error
        /// </summary>
        PresentationError = 7,

        /// <summary>
        /// Pending
        /// </summary>
        Pending = 8,

        /// <summary>
        /// Judging &amp; Running
        /// </summary>
        Running = 9,

        /// <summary>
        /// Undefined Error
        /// </summary>
        UndefinedError = 10,

        /// <summary>
        /// Accepted
        /// </summary>
        Accepted = 11,
    }
}
