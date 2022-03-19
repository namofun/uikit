using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace Microsoft.ApplicationInsights.Extensibility
{
    public class LogicAppsInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is DependencyTelemetry dependency
                && dependency.Type == "Http"
                && dependency.Target.EndsWith(".logic.azure.com", StringComparison.OrdinalIgnoreCase))
            {
                dependency.Target = "logic.azure.com";

                if (dependency.TryGetOperationDetail("HttpResponse", out object rawHttpResponse)
                    && rawHttpResponse is HttpResponseMessage httpResponse)
                {
                    AddToProperty(httpResponse, "x-ms-workflow-run-id", dependency, "RunHistoryId");
                    AddToProperty(httpResponse, "x-ms-workflow-system-id", dependency, "WorkflowRouting");
                    AddToProperty(httpResponse, "x-ms-workflow-version", dependency, "WorkflowVersion");
                    AddToProperty(httpResponse, "x-ms-request-id", dependency, "ServerRequestId");
                    if (TryGetSingleHeader(httpResponse, "x-ms-workflow-name", out var workflowName))
                    {
                        dependency.Name = "CALL " + workflowName;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetSingleHeader(
            HttpResponseMessage httpResponse,
            string headerName,
            [NotNullWhen(true)] out string? headerValue)
        {
            if (httpResponse.Headers.TryGetValues(headerName, out var headerValues)
                && headerValues.FirstOrDefault() is string headerValue2
                && !string.IsNullOrEmpty(headerValue2))
            {
                headerValue = headerValue2;
                return true;
            }
            else
            {
                headerValue = null;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AddToProperty(
            HttpResponseMessage httpResponse, string headerName,
            DependencyTelemetry dependency, string propertyName)
        {
            if (TryGetSingleHeader(httpResponse, headerName, out var headerValue))
            {
                dependency.Properties[propertyName] = headerValue;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
