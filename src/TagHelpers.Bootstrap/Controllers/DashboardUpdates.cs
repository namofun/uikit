using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace SatelliteSite.Substrate.Dashboards
{
    /// <summary>
    /// Consume dashboard updates.
    /// </summary>
    public class DashboardUpdates : INotification
    {
        private readonly Dictionary<string, object> _values;

        /// <summary>
        /// The HTTP Context
        /// </summary>
        public HttpContext Context { get; }

        /// <summary>
        /// The present values
        /// </summary>
        public IReadOnlyDictionary<string, object> Values => _values;

        /// <summary>
        /// Initialize an instance of <see cref="DashboardUpdates"/>.
        /// </summary>
        /// <param name="context">The http context.</param>
        public DashboardUpdates(HttpContext context)
        {
            Context = context;
            _values = new Dictionary<string, object>();
        }

        /// <summary>
        /// Add the values to results.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(string key, object value)
        {
            _values.Add(key, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _values.ToJson();
        }
    }
}
