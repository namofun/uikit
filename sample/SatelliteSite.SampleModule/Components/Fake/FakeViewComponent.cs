using Microsoft.AspNetCore.Mvc;

namespace SatelliteSite.SampleModule.Components.Fake
{
    public class FakeViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("Default");
        }
    }
}
