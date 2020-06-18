using Microsoft.AspNetCore.Mvc;

namespace SatelliteSite.SampleModule.Controllers
{
    [Area("Sample")]
    [Route("[area]/[controller]/[action]")]
    public class MainController : Controller2
    {
        public IActionResult Home()
        {
            return View();
        }
    }
}
