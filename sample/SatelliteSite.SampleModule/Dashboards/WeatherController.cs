using Microsoft.AspNetCore.Mvc;

namespace SatelliteSite.SampleModule.Dashboards
{
    [Area("Dashboard")]
    [Route("[area]/[controller]")]
    public class WeatherController : ViewControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult Change()
        {
            return View();
        }
    }
}
