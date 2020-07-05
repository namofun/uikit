using Microsoft.AspNetCore.Mvc;
using SatelliteSite.SampleModule.Models;
using SatelliteSite.SampleModule.Services;
using System.Linq;

namespace SatelliteSite.SampleModule.Apis
{
    [Area("Api")]
    [Route("[area]/[controller]")]
    public class WeatherController : ApiControllerBase
    {
        private ForecastService Service { get; }

        public WeatherController(ForecastService service)
        {
            Service = service;
        }

        [HttpGet("[action]")]
        public ActionResult<WeatherForecast[]> Forecast()
        {
            return Service.Forecast().ToArray();
        }
    }
}
