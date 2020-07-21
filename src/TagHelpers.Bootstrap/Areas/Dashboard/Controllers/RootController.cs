using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.AspNetCore.Mvc.Dashboards
{
    [Area("Dashboard")]
    [Route("[area]/misc/[action]")]
    //[Authorize("HasDashboard")]
    public class RootController : ViewControllerBase
    {
        private string? EndpointResults { get; set; }
        private string? VersionResults { get; set; }


        [HttpGet("/[area]")]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Endpoints(
            [FromServices] DfaGraphWriter writer,
            [FromServices] IOptions<RouteOptions> options)
        {
            if (EndpointResults == null)
            {
                using var sw = new StringWriter();
                var edss = options.Value.Private<ICollection<EndpointDataSource>>("_endpointDataSources");
                var eds = new CompositeEndpointDataSource(edss);
                writer.Write(eds, sw);
                EndpointResults = sw.ToString();
            }

            return Content(EndpointResults, "text/plain");
        }


        [HttpGet]
        public IActionResult Versions()
        {
            if (VersionResults == null)
            {
                var text = new StringBuilder();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var gitVersion = assembly.GetCustomAttributes(false)
                        .OfType<GitVersionAttribute>()
                        .SingleOrDefault();
                    if (gitVersion == null) continue;
                    text.Append(assembly.FullName)
                        .AppendLine($", Commit={gitVersion.Version}, Branch={gitVersion.Branch}");
                }

                VersionResults = text.ToString();
            }

            return Content(VersionResults, "text/plain");
        }
    }
}
