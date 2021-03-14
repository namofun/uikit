using SatelliteSite.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SatelliteSite.Services
{
    public class NullAuditlogger : IAuditlogger
    {
        public Task LogAsync(AuditlogType type, string userName, DateTimeOffset now, string action, string? target, string? extra, int? cid)
        {
            return Task.CompletedTask;
        }

        public Task<IPagedList<Auditlog>> ViewLogsAsync(int? cid, int page, int pageCount)
        {
            return Task.FromResult<IPagedList<Auditlog>>(new PagedViewList<Auditlog>(Array.Empty<Auditlog>(), 0, 0, 0));
        }
    }
}
