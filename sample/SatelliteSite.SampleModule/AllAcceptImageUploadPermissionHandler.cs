using MediatR;
using SatelliteSite.Substrate.Dashboards;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.SampleModule
{
    public class AllAcceptImageUploadPermissionHandler : INotificationHandler<ImageUploadPermission>
    {
        public Task Handle(ImageUploadPermission notification, CancellationToken cancellationToken)
        {
            notification.Handled = true;
            return Task.CompletedTask;
        }
    }
}
