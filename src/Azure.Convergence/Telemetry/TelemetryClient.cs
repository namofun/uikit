﻿using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights
{
    /// <inheritdoc cref="ITelemetryClient" />
    public class ApplicationInsightsTelemetryClient : ITelemetryClient
    {
        private readonly TelemetryClient _appInsights;
        private readonly JavaScriptSnippet _javaScriptSnippet;

        /// <summary>
        /// Initialize the <see cref="ApplicationInsightsTelemetryClient"/>.
        /// </summary>
        /// <param name="telemetryClient">The inner telemetry client.</param>
        /// <param name="javaScriptSnippet">The javascript snippet.</param>
        public ApplicationInsightsTelemetryClient(
            TelemetryClient telemetryClient,
            JavaScriptSnippet javaScriptSnippet)
        {
            _appInsights = telemetryClient;
            _javaScriptSnippet = javaScriptSnippet;
        }

        /// <inheritdoc />
        public void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string? message = null, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            _appInsights.TrackAvailability(name, timeStamp, duration, runLocation, success, message, properties, metrics);
        }

        /// <inheritdoc />
        public void TrackDependency(string dependencyTypeName, string target, string operationName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
        {
            _appInsights.TrackDependency(dependencyTypeName, target, operationName, data, startTime, duration, resultCode, success);
        }

        /// <inheritdoc />
        public void TrackDependency(string dependencyTypeName, string operationName, string data, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
            _appInsights.TrackDependency(dependencyTypeName, operationName, data, startTime, duration, success);
        }

        /// <inheritdoc />
        public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            _appInsights.TrackEvent(eventName, properties, metrics);
        }

        /// <inheritdoc />
        public void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            _appInsights.TrackException(exception, properties, metrics);
        }

        /// <inheritdoc />
        public void TrackPageView(string name)
        {
            _appInsights.TrackPageView(name);
        }

        /// <inheritdoc />
        public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
            _appInsights.TrackRequest(name, startTime, duration, responseCode, success);
        }

        /// <inheritdoc />
        public void TrackTrace(string message, LogLevel severityLevel, IDictionary<string, string>? properties = null)
        {
            _appInsights.TrackTrace(message, GetSeverityLevel(severityLevel), properties);
        }

        /// <summary>
        /// Converts the <see cref="LogLevel"/> into corresponding Application insights <see cref="SeverityLevel"/>.
        /// </summary>
        /// <param name="logLevel">Logging log level.</param>
        /// <returns>Application insights corresponding SeverityLevel for the LogLevel.</returns>
        private static SeverityLevel GetSeverityLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Critical => SeverityLevel.Critical,
                LogLevel.Error => SeverityLevel.Error,
                LogLevel.Warning => SeverityLevel.Warning,
                LogLevel.Information => SeverityLevel.Information,
                _ => SeverityLevel.Verbose,
            };
        }

        /// <inheritdoc />
        public string GetHeadJavascript()
        {
            return _javaScriptSnippet.FullScript;
        }

        private IOperationHolder<DependencyTelemetry> StartInProcOperationScope(string scopeName)
        {
            return _appInsights.StartOperation(new DependencyTelemetry()
            {
                Name = scopeName,
                Type = "InProc",
                Context = { Operation = { Name = scopeName } }
            });
        }

        /// <inheritdoc />
        public void TrackScope(string scopeName, Action scopeFunc)
        {
            using var operationHolder = StartInProcOperationScope(scopeName);
            try
            {
                scopeFunc();
            }
            catch
            {
                operationHolder.Telemetry.Success = false;
                throw;
            }
            finally
            {
                _appInsights.StopOperation(operationHolder);
            }
        }

        /// <inheritdoc />
        public async Task TrackScope(string scopeName, Func<Task> scopeFunc)
        {
            using var operationHolder = StartInProcOperationScope(scopeName);
            try
            {
                await scopeFunc();
            }
            catch
            {
                operationHolder.Telemetry.Success = false;
                throw;
            }
            finally
            {
                _appInsights.StopOperation(operationHolder);
            }
        }

        /// <inheritdoc />
        public async Task<T> TrackScope<T>(string scopeName, Func<Task<T>> scopeFunc)
        {
            using var operationHolder = StartInProcOperationScope(scopeName);
            try
            {
                return await scopeFunc();
            }
            catch
            {
                operationHolder.Telemetry.Success = false;
                throw;
            }
            finally
            {
                _appInsights.StopOperation(operationHolder);
            }
        }

        /// <inheritdoc />
        public IDependencyTracker StartOperation(string dependencyTypeName, string? target, string operationName)
        {
            return new ApplicationInsightsDependencyTracker(_appInsights, dependencyTypeName, target, operationName);
        }

        /// <inheritdoc />
        public void StopOperation(IDependencyTracker tracker)
        {
            if (tracker is ApplicationInsightsDependencyTracker tracker1)
            {
                _appInsights.StopOperation(tracker1.OperationHolder);
            }
        }
    }
}
