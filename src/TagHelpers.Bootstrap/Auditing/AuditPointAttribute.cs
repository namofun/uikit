﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using SatelliteSite;
using System;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// The attribute to specify auditlog point in <see cref="HttpContext"/>.
    /// </summary>
    public sealed class AuditPointAttribute : Attribute, IActionFilter
    {
        /// <summary>
        /// The auditlog type
        /// </summary>
        private readonly AuditlogType _type;

        /// <summary>
        /// The name of http context item
        /// </summary>
        internal const string AuditlogTypeName = nameof(AuditlogType);

        /// <summary>
        /// Instantiate an <see cref="AuditPointAttribute"/>.
        /// </summary>
        /// <param name="type">The auditlog type.</param>
        public AuditPointAttribute(AuditlogType type) => _type = type;

        /// <inheritdoc />
        public void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.Items.Remove(AuditlogTypeName);
        }

        /// <inheritdoc />
        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Items[AuditlogTypeName] = _type;
        }
    }
}
