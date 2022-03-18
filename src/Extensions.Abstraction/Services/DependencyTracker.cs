using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Diagnostics
{
    /// <summary>
    /// The interface that holds and tracks information about the collected dependency.
    /// </summary>
    public interface IDependencyTracker : IDisposable
    {
        /// <summary>
        /// Gets or sets the dependency type name.
        /// </summary>
        string DependencyType { get; }

        /// <summary>
        /// Gets or sets target of dependency call. SQL server name, url host, etc.
        /// </summary>
        string? DependencyTarget { get; }

        /// <summary>
        /// Gets or sets operation name of dependency call. HTTP method, SQL operation, etc.
        /// </summary>
        string OperationName { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the dependency call was successful or not.
        /// </summary>
        bool? Success { get; set; }

        /// <summary>
        /// Gets or sets data associated with the current dependency instance. Command name/statement for SQL dependency, URL for http dependency.
        /// </summary>
        string? Data { get; set; }

        /// <summary>
        /// Gets or sets the Result Code.
        /// </summary>
        string? ResultCode { get; set; }

        /// <summary>
        /// Gets a dictionary of application-defined event metrics.
        /// </summary>
        IDictionary<string, double> Metrics { get; }

        /// <summary>
        /// Gets a dictionary of application-defined property names and values providing additional information about this remote dependency.
        /// </summary>
        IDictionary<string, string> Properties { get; }
    }
}
