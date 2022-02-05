using Microsoft.AspNetCore.Mvc;
using SatelliteSite.SampleModule.Services;

namespace SatelliteSite.SampleModule.Dashboards
{
    [Area("Dashboard")]
    [Route("[area]/[controller]")]
    public class WeatherController : ViewControllerBase
    {
        private ForecastService Service { get; }

        public WeatherController(ForecastService service)
        {
            Service = service;
        }


        [HttpGet("[action]")]
        public IActionResult Change()
        {
            return View(Service.Forecast());
        }
    }
}
