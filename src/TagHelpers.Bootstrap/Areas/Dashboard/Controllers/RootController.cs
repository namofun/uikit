using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SatelliteSite.Substrate.Dashboards
{
    [Area("Dashboard")]
    [Route("[area]/[action]")]
    //[Authorize("HasDashboard")]
    public class RootController : ViewControllerBase
    {
        [HttpGet("/[area]")]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Endpoints(
            [FromServices] IOptions<RouteOptions> options)
        {
            var edss = options.Value.Private<ICollection<EndpointDataSource>>("_endpointDataSources");
            return View(edss);
        }


        [HttpGet]
        public IActionResult Versions()
        {
            var lst = new List<LoadedModulesModel>();
                
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var gitVersion = assembly.GetCustomAttributes(false)
                    .OfType<GitVersionAttribute>()
                    .SingleOrDefault();
                var asName = assembly.GetName();

                lst.Add(new LoadedModulesModel
                {
                    AssemblyName = asName.Name,
                    Branch = gitVersion?.Branch,
                    CommitLong = gitVersion?.Version,
                    PublicKey = asName.GetPublicKeyToken()?.ToHexDigest(true),
                    Version = asName.Version?.ToString(),
                });
            }

            return View(lst);
        }
    }
}
