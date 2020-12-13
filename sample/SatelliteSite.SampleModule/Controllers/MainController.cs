using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.SampleModule.Services;

namespace SatelliteSite.SampleModule.Controllers
{
    [Area("Sample")]
    [Route("[area]/[controller]/[action]")]
    public class MainController : ViewControllerBase
    {
        private ForecastService Service { get; }

        public MainController(ForecastService service)
        {
            Service = service;
        }

        public IActionResult Home()
        {
            return View(Service.Forecast());
        }

        [Authorize]
        public IActionResult Claims(string roleName)
        {
            return Json(User.IsInRole(roleName));
        }
    }
}
