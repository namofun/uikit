using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.ApplicationInsights
{
    internal sealed class ApplicationInsightsDependencyTracker : IDependencyTracker
    {
        public IOperationHolder<DependencyTelemetry> OperationHolder { get; }

        public ApplicationInsightsDependencyTracker(
            TelemetryClient telemetryClient,
            string dependencyTypeName,
            string? target,
            string operationName)
        {
            OperationHolder = telemetryClient.StartOperation(new DependencyTelemetry()
            {
                Target = target,
                Name = operationName,
                Type = dependencyTypeName,
            });
        }

        public string DependencyType => OperationHolder.Telemetry.Type;

        public string? DependencyTarget => OperationHolder.Telemetry.Target;

        public string OperationName => OperationHolder.Telemetry.Name;

        public IDictionary<string, double> Metrics => OperationHolder.Telemetry.Metrics;

        public IDictionary<string, string> Properties => OperationHolder.Telemetry.Properties;

        public bool? Success
        {
            get => OperationHolder.Telemetry.Success;
            set => OperationHolder.Telemetry.Success = value;
        }

        public string? Data
        {
            get => OperationHolder.Telemetry.Data;
            set => OperationHolder.Telemetry.Data = value;
        }

        public string? ResultCode
        {
            get => OperationHolder.Telemetry.ResultCode;
            set => OperationHolder.Telemetry.ResultCode = value;
        }

        public void Dispose()
        {
            OperationHolder.Dispose();
        }
    }
}
