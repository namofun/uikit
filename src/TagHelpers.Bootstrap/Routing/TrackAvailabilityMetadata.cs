namespace Microsoft.AspNetCore.Routing
{
    /// <summary>
    /// The track availability description.
    /// </summary>
    public enum TrackAvailability
    {
        /// <summary>The default availability</summary>
        Default = 0,

        /// <summary>The fallback availability</summary>
        Fallback = 1,

        /// <summary>The error handler availability</summary>
        ErrorHandler = 2,
    }

    /// <summary>
    /// The metadata for endpoints to provide track informations.
    /// </summary>
    public class TrackAvailabilityMetadata
    {
        /// <summary>The track level</summary>
        public TrackAvailability Track { get; }

        /// <summary>
        /// Instantiate a <see cref="TrackAvailabilityMetadata"/>.
        /// </summary>
        /// <param name="level">The track level.</param>
        private TrackAvailabilityMetadata(TrackAvailability level) => Track = level;

        /// <summary>The default availability</summary>
        public static readonly TrackAvailabilityMetadata Default = new(TrackAvailability.Default);

        /// <summary>The error handler availability</summary>
        public static readonly TrackAvailabilityMetadata ErrorHandler = new(TrackAvailability.ErrorHandler);

        /// <summary>The fallback availability</summary>
        public static readonly TrackAvailabilityMetadata Fallback = new(TrackAvailability.Fallback);
    }
}
