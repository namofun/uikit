using Microsoft.AspNetCore.Mvc;
using SatelliteSite.Services;

namespace SatelliteSite.Substrate.Dashboards
{
    [Area("Dashboard")]
    public class ApiDocController : ViewControllerBase
    {
        [SuppressLink]
        [Route("/api/doc/{name}")]
        public IActionResult Display(
            string name,
            [FromServices] IApiDocumentProvider api)
        {
            api.GetDocument(name, out string title, out string spec);
            ViewBag.Title = title;
            ViewBag.Spec = spec;
            return View("/Areas/Dashboard/Views/ApiDoc/Display.cshtml");
        }
    }
}
