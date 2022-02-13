using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

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
            IApiDocument document = api.GetDocument(name);
            ApiSpecificationType? type = null;

            if (Request.Headers.Accept.Count == 0)
            {
                Request.Headers.Accept = new("*/*");
            }

            foreach (string mime in Request.Headers.GetCommaSeparatedValues("accept"))
            {
                switch (mime)
                {
                    case "text/json":
                    case "application/json":
                    case "application/swagger+json":
                    case "application/swagger+json;ver=3":
                        type ??= ApiSpecificationType.SwaggerV3Json;
                        break;

                    case "application/swagger+json;ver=2":
                        type ??= ApiSpecificationType.SwaggerV2Json;
                        break;

                    case "text/yaml":
                    case "text/swagger+yaml":
                    case "text/swagger+yaml;ver=3":
                        type ??= ApiSpecificationType.SwaggerV3Yaml;
                        break;

                    case "text/swagger+yaml;ver=2":
                        type ??= ApiSpecificationType.SwaggerV2Yaml;
                        break;

                    case "text/html":
                    case "text/html;charset=utf-8":
                    case "*/*":
                        type ??= ApiSpecificationType.Unknown;
                        break;
                }
            }

            if (type == ApiSpecificationType.Unknown)
            {
                ViewBag.Title = document.Title;
                ViewBag.Spec = document.GetSpecification(ApiSpecificationType.SwaggerV2Json);
                return View("/Areas/Dashboard/Views/ApiDoc/Display.cshtml");
            }
            else if (type == ApiSpecificationType.SwaggerV3Json || type == ApiSpecificationType.SwaggerV2Json)
            {
                return Content(document.GetSpecification(type.Value), "application/json");
            }
            else if (type == ApiSpecificationType.SwaggerV3Yaml || type == ApiSpecificationType.SwaggerV2Yaml)
            {
                return Content(document.GetSpecification(type.Value), "text/yaml");
            }
            else
            {
                return StatusCode(StatusCodes.Status406NotAcceptable);
            }
        }
    }
}
