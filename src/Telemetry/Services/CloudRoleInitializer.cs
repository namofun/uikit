#nullable enable

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace SatelliteSite.TelemetryModule.Services
{
    public class CloudRoleInitializer : ITelemetryInitializer
    {
        public string? RoleName { get; set; }

        public string? RoleInstance { get; set; }

        public CloudRoleInitializer(string? roleName, string? roleInstance)
        {
            RoleName = roleName;
            RoleInstance = roleInstance;
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (RoleName != null)
            {
                telemetry.Context.Cloud.RoleName = RoleName;
            }

            if (RoleInstance != null)
            {
                telemetry.Context.Cloud.RoleInstance = RoleInstance;
            }
        }
    }
}
