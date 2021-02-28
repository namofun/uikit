using Microsoft.AspNetCore.Mvc;

namespace SatelliteSite.SampleConnector.Controllers
{
    [Area("Sample")]
    [Route("[area]/[controller]/[action]")]
    public class TestController : ViewControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
