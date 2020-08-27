#nullable disable
using System;

namespace SatelliteSite.Entities
{
    /// <summary>
    /// The auditlog for recording operations on websites.
    /// </summary>
    public class Auditlog
    {
        /// <summary>
        /// The ID of log.
        /// </summary>
        public int LogId { get; set; }

        /// <summary>
        /// The time of event happened.
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// The username who emitted this event.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The contest id for events.
        /// </summary>
        public int? ContestId { get; set; }

        /// <summary>
        /// The data type for this event source.
        /// </summary>
        public AuditlogType DataType { get; set; }

        /// <summary>
        /// The id of data target.
        /// </summary>
        public string DataId { get; set; }

        /// <summary>
        /// The target action.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The extra comment on events.
        /// </summary>
        public string ExtraInfo { get; set; }
    }
}
