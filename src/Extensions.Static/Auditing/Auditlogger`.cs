using SatelliteSite.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SatelliteSite.Services
{
    /// <summary>
    /// The logger for auditing.
    /// </summary>
    public interface IAuditlogger
    {
        /// <summary>
        /// Log the information.
        /// </summary>
        /// <param name="type">The log type.</param>
        /// <param name="userName">The caller name.</param>
        /// <param name="now">The timestamp.</param>
        /// <param name="action">The action.</param>
        /// <param name="target">The data target.</param>
        /// <param name="extra">The extra comment.</param>
        /// <param name="cid">The contest id.</param>
        /// <returns>The task.</returns>
        Task LogAsync(
            AuditlogType type,
            string userName,
            DateTimeOffset now,
            string action,
            string target,
            string extra,
            int? cid);

        /// <summary>
        /// Get the auditlogs.
        /// </summary>
        /// <param name="cid">The contest id.</param>
        /// <param name="page">The page to show.</param>
        /// <param name="pageCount">The count of pages to show.</param>
        /// <returns>The task with auditlogs.</returns>
        Task<PagedViewList<Auditlog>> ViewLogsAsync(
            int? cid,
            int page,
            int pageCount);
    }
}
