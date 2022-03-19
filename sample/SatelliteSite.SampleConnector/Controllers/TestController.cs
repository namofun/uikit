using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.SampleConnector.Services;
using System.Threading.Tasks;

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

        [Authorize]
        public async Task<IActionResult> Subscriptions(
            [FromServices] AzureManagementClient client)
        {
            return Json(await client.GetSubscriptionsAsync());
        }

        [Authorize]
        public async Task<IActionResult> LogicApps(
            [FromServices] LogicAppsClient client)
        {
            return Content(await client.InvokeAsync());
        }
    }
}
