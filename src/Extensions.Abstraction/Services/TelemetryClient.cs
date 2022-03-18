using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Diagnostics
{
    /// <summary>
    /// Defines a client for telemetry.
    /// </summary>
    public interface ITelemetryClient
    {
        /// <summary>
        /// Send information about availability of an application.
        /// </summary>
        /// <param name="name">Availability test name.</param>
        /// <param name="timeStamp">The time when the availability was captured.</param>
        /// <param name="duration">The time taken for the availability test to run.</param>
        /// <param name="runLocation">Name of the location the availability test was run from.</param>
        /// <param name="success">True if the availability test ran successfully.</param>
        /// <param name="message">Error message on availability test run failure.</param>
        /// <param name="properties">Named string values you can use to classify and search for this availability telemetry.</param>
        /// <param name="metrics">Additional values associated with this availability telemetry.</param>
        void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string? message = null, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);

        /// <summary>
        /// Send information about an external dependency (outgoing call) in the application.
        /// </summary>
        /// <param name="dependencyTypeName">External dependency type. Very low cardinality value for logical grouping and interpretation of fields. Examples are SQL, Azure table, and HTTP.</param>
        /// <param name="target">External dependency target.</param>
        /// <param name="operationName">Name of the command initiated with this dependency call. Low cardinality value. Examples are stored procedure name and URL path template.</param>
        /// <param name="data">Command initiated by this dependency call. Examples are SQL statement and HTTP URL's with all query parameters.</param>
        /// <param name="startTime">The time when the dependency was called.</param>
        /// <param name="duration">The time taken by the external dependency to handle the call.</param>
        /// <param name="resultCode">Result code of dependency call execution.</param>
        /// <param name="success">True if the dependency call was handled successfully.</param>
        void TrackDependency(string dependencyTypeName, string target, string operationName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success);

        /// <summary>
        /// Send information about an external dependency (outgoing call) in the application.
        /// </summary>
        /// <param name="dependencyTypeName">External dependency type. Very low cardinality value for logical grouping and interpretation of fields. Examples are SQL, Azure table, and HTTP.</param>
        /// <param name="operationName">Name of the command initiated with this dependency call. Low cardinality value. Examples are stored procedure name and URL path template.</param>
        /// <param name="data">Command initiated by this dependency call. Examples are SQL statement and HTTP URL's with all query parameters.</param>
        /// <param name="startTime">The time when the dependency was called.</param>
        /// <param name="duration">The time taken by the external dependency to handle the call.</param>
        /// <param name="success">True if the dependency call was handled successfully.</param>
        void TrackDependency(string dependencyTypeName, string operationName, string data, DateTimeOffset startTime, TimeSpan duration, bool success);

        /// <summary>
        /// Start tracking an external dependency (outgoing call) in the application.
        /// </summary>
        /// <param name="dependencyTypeName">External dependency type. Very low cardinality value for logical grouping and interpretation of fields. Examples are SQL, Azure table, and HTTP.</param>
        /// <param name="target">External dependency target.</param>
        /// <param name="operationName">Name of the command initiated with this dependency call. Low cardinality value. Examples are stored procedure name and URL path template.</param>
        /// <returns>The dependency tracker. Caller must dispose the tracker to stop the operation.</returns>
        IDependencyTracker StartOperation(string dependencyTypeName, string? target, string operationName);

        /// <summary>
        /// Send an event telemetry for display in Diagnostic Search and in the Analytics Portal.
        /// </summary>
        /// <param name="eventName">A name for the event.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);

        /// <summary>
        /// Send an exception telemetry for display in Diagnostic Search.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="properties">Named string values you can use to classify and search for this exception.</param>
        /// <param name="metrics">Additional values associated with this exception.</param>
        void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);

        /// <summary>
        /// Send information about the page viewed in the application.
        /// </summary>
        /// <param name="name">Name of the page.</param>
        void TrackPageView(string name);

        /// <summary>
        /// Send information about a request handled by the application.
        /// </summary>
        /// <param name="name">The request name.</param>
        /// <param name="startTime">The time when the page was requested.</param>
        /// <param name="duration">The time taken by the application to handle the request.</param>
        /// <param name="responseCode">The response status code.</param>
        /// <param name="success">True if the request was handled successfully by the application.</param>
        void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success);

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="severityLevel">Trace severity level.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        void TrackTrace(string message, LogLevel severityLevel, IDictionary<string, string>? properties = null);

        /// <summary>
        /// Track the start, stop, correlation and exception in scope.
        /// </summary>
        /// <param name="scopeName">Name of scope.</param>
        /// <param name="scopeFunc">Actual action of scope.</param>
        void TrackScope(string scopeName, Action scopeFunc);

        /// <summary>
        /// Track the start, stop, correlation and exception in scope.
        /// </summary>
        /// <param name="scopeName">Name of scope.</param>
        /// <param name="scopeFunc">Actual action of scope.</param>
        Task TrackScope(string scopeName, Func<Task> scopeFunc);

        /// <summary>
        /// Track the start, stop, correlation and exception in scope.
        /// </summary>
        /// <param name="scopeName">Name of scope.</param>
        /// <param name="scopeFunc">Actual action of scope.</param>
        Task<T> TrackScope<T>(string scopeName, Func<Task<T>> scopeFunc);

        /// <summary>
        /// Gets the javascript segment for rendering in head.
        /// </summary>
        /// <returns>The head javascript.</returns>
        string GetHeadJavascript();
    }

    /// <inheritdoc cref="ITelemetryClient" />
    public class NullTelemetryClient : ITelemetryClient
    {
        /// <inheritdoc />
        public void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string? message = null, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
        }

        /// <inheritdoc />
        public void TrackDependency(string dependencyTypeName, string target, string operationName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
        {
        }

        /// <inheritdoc />
        public void TrackDependency(string dependencyTypeName, string operationName, string data, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
        }

        /// <inheritdoc />
        public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
        }

        /// <inheritdoc />
        public void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
        }

        /// <inheritdoc />
        public void TrackPageView(string name)
        {
        }

        /// <inheritdoc />
        public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
        }

        /// <inheritdoc />
        public void TrackTrace(string message, LogLevel severityLevel, IDictionary<string, string>? properties = null)
        {
        }

        /// <inheritdoc />
        public string GetHeadJavascript()
        {
            return string.Empty;
        }

        /// <inheritdoc />
        public void TrackScope(string scopeName, Action scopeFunc)
        {
            scopeFunc();
        }

        /// <inheritdoc />
        public Task TrackScope(string scopeName, Func<Task> scopeFunc)
        {
            return scopeFunc();
        }

        /// <inheritdoc />
        public Task<T> TrackScope<T>(string scopeName, Func<Task<T>> scopeFunc)
        {
            return scopeFunc();
        }

        /// <inheritdoc />
        public IDependencyTracker StartOperation(string dependencyTypeName, string? target, string operationName)
        {
            return new NullDependencyTracker(dependencyTypeName, target, operationName);
        }

        private sealed class NullDependencyTracker : IDependencyTracker
        {
            private Dictionary<string, double>? _metrics;
            private Dictionary<string, string>? _properties;

            public string DependencyType { get; }
            public string DependencyTarget { get; }
            public string OperationName { get; }
            public bool? Success { get; set; }
            public string? Data { get; set; }
            public string? ResultCode { get; set; }
            public IDictionary<string, double> Metrics => _metrics ??= new();
            public IDictionary<string, string> Properties => _properties ??= new();
            public void Dispose() { }

            public NullDependencyTracker(string type, string target, string name)
            {
                DependencyType = type;
                DependencyTarget = target;
                OperationName = name;
            }
        }
    }
}
