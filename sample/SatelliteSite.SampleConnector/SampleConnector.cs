using Microsoft.AspNetCore.Mvc;

[assembly: AffiliateTo(
    typeof(SatelliteSite.SampleConnector.SampleConnector),
    typeof(SatelliteSite.SampleModule.SampleModule))]

namespace SatelliteSite.SampleConnector
{
    public class SampleConnector : AbstractConnector
    {
        public override string Area => "Sample";

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Submenu(MenuNameDefaults.DashboardUsers, menu =>
            {
                menu.HasEntry(-1000)
                    .HasLink("Sample", "Test", "Index")
                    .HasTitle(string.Empty, "Sample Connector");
            });
        }
    }
}
