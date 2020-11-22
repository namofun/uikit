using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.Entities;
using SatelliteSite.Services;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Several audit point extension methods.
    /// </summary>
    public static class AuditPointExtensions
    {
        /// <summary>
        /// Audit the status.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/></param>
        /// <param name="action">The action name.</param>
        /// <param name="target">The target name.</param>
        /// <param name="extra">The extra info.</param>
        /// <returns>A task representing the audit process.</returns>
        public static Task AuditAsync(this HttpContext context,
            string action, string? target, string? extra = null)
        {
            if (!context.Items.TryGetValue(AuditPointAttribute.AuditlogTypeName, out object? typer))
                return Task.FromException(new InvalidOperationException("No audit point specified."));
            int? cid = null;
            if (context.Items.TryGetValue(nameof(cid), out object? cidd))
                cid = (int?)cidd;
            return AuditAsync(context,
                (AuditlogType)typer, cid, context.User.GetUserName() ?? "TOURIST",
                action, target, extra);
        }

        /// <summary>
        /// Audit the status.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/></param>
        /// <param name="typer">The auditlog type.</param>
        /// <param name="cid">The contest id.</param>
        /// <param name="username">The username.</param>
        /// <param name="action">The action name.</param>
        /// <param name="target">The target name.</param>
        /// <param name="extra">The extra info.</param>
        /// <returns>A task representing the audit process.</returns>
        public static Task AuditAsync(this HttpContext context,
            AuditlogType typer, int? cid, string username,
            string action, string? target, string? extra = null)
        {
            return context.RequestServices.GetRequiredService<IAuditlogger>()
                .LogAsync(typer, username, DateTimeOffset.Now, action, target, extra, cid);
        }
    }
}
