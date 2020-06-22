using Microsoft.AspNetCore.Mvc;

namespace SatelliteSite.SampleModule.Controllers
{
    [Area("Sample")]
    [Route("[area]/[controller]/[action]")]
    public class MainController : ViewControllerBase
    {
        public IActionResult Home()
        {
            return View();
        }
    }
}
