using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;
using System.Globalization;
using System.IO;

namespace SatelliteSite.Substrate.Dashboards
{
    [Area("Dashboard")]
    public class ApiDocController : ViewControllerBase
    {
        private static readonly IMemoryCache _apiDocCache =
            new MemoryCache(new MemoryCacheOptions { Clock = new SystemClock() });
        private readonly ISwaggerProvider _swaggerGen;

        public ApiDocController(ISwaggerProvider swaggerGen)
        {
            _swaggerGen = swaggerGen;
        }

        [SuppressLink]
        public IActionResult Display(string name)
        {
            var s = _apiDocCache.GetOrCreate(name, entry =>
            {
                var document = _swaggerGen.GetSwagger(name);
                var title = document.Info.Title;
                using var textWriter = new StringWriter(CultureInfo.InvariantCulture);
                var jsonWriter = new OpenApiJsonWriter(textWriter);
                document.SerializeAsV2(jsonWriter);
                var spec = textWriter.ToString();
                return (title, spec);
            });

            ViewBag.Title = s.title;
            ViewBag.Spec = s.spec;
            return View("/Areas/Dashboard/Views/ApiDoc/Display.cshtml");
        }
    }
}
