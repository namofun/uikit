using Microsoft.AspNetCore.Mvc;
using SatelliteSite.SampleModule.Services;
using System.Threading.Tasks;

namespace SatelliteSite.SampleModule.Components.Weather
{
    public class WeatherViewComponent : ViewComponent
    {
        private readonly ForecastService _service;

        public WeatherViewComponent(ForecastService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            await Task.CompletedTask;
            return View("Default", _service.Forecast());
        }
    }
}
