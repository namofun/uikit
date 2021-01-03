using Microsoft.EntityFrameworkCore;
using SatelliteSite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.Services
{
    /// <summary>
    /// Entity Framework Core implementation for <see cref="IAuditlogger"/>.
    /// </summary>
    /// <typeparam name="TContext">The database context.</typeparam>
    public class Auditlogger<TContext> : IAuditlogger
        where TContext : DbContext
    {
        /// <summary>
        /// The default context.
        /// </summary>
        private TContext Context { get; }

        /// <summary>
        /// The default set.
        /// </summary>
        private DbSet<Auditlog> Auditlogs => Context.Set<Auditlog>();

        /// <summary>
        /// Constructs a logger.
        /// </summary>
        /// <param name="context">The database context.</param>
        public Auditlogger(TContext context)
        {
            Context = context;
        }

        /// <inheritdoc />
        public Task LogAsync(
            AuditlogType type, string userName, DateTimeOffset now,
            string action, string? target, string? extra, int? cid)
        {
            Auditlogs.Add(new Auditlog
            {
                Action = action,
                Time = now,
                DataId = target,
                DataType = type,
                ContestId = cid,
                ExtraInfo = extra,
                UserName = userName,
            });

            return Context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public Task<IPagedList<Auditlog>> ViewLogsAsync(int? cid, int page, int pageCount)
        {
            return Auditlogs
                .Where(a => a.ContestId == cid)
                .OrderByDescending(a => a.LogId)
                .ToPagedListAsync(page, pageCount);
        }
    }
}
