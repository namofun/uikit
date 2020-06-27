namespace Microsoft.AspNetCore.Mvc.Dashboards
{
    [Area("Dashboard")]
    public class RootController : ViewControllerBase
    {
        [HttpGet("/[area]")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
