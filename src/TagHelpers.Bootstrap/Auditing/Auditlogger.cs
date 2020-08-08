using Microsoft.EntityFrameworkCore;
using SatelliteSite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
            string? target,
            string? extra,
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
        public async Task<PagedViewList<Auditlog>> ViewLogsAsync(int? cid, int page, int pageCount)
        {
            var count = await Auditlogs
                .Where(a => a.ContestId == cid)
                .CountAsync();

            var query = await Auditlogs
                .Where(a => a.ContestId == cid)
                .OrderByDescending(a => a.LogId)
                .Skip((page - 1) * pageCount)
                .Take(pageCount)
                .ToListAsync();

            return new PagedViewList<Auditlog>(query, page, count, pageCount);
        }
    }
}
