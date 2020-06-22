using Microsoft.AspNetCore.Mvc;
using SatelliteSite.SampleModule.Models;

namespace SatelliteSite.SampleModule.Apis
{
    [Area("Api")]
    [Route("[area]/[controller]")]
    public class WeatherController : ApiControllerBase
    {
        [HttpGet("[action]")]
        public ActionResult<WeatherModel> Forecast()
        {
            return new WeatherModel
            {
                Name = "The world",
                Temperature = 2
            };
        }
    }
}
