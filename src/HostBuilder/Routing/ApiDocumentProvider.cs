using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;
using System.Globalization;
using System.IO;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class ApiDocumentProvider : IApiDocumentProvider
    {
        private readonly IMemoryCache _apiDocCache =
            new MemoryCache(new MemoryCacheOptions { Clock = new SystemClock() });
        private readonly ISwaggerProvider _swaggerGen;

        public ApiDocumentProvider(ISwaggerProvider swaggerGen)
        {
            _swaggerGen = swaggerGen;
        }

        public void GetDocument(string name, out string Title, out string Spec)
        {
            (Title, Spec) = _apiDocCache.GetOrCreate(name, entry =>
            {
                var document = _swaggerGen.GetSwagger(name);
                var title = document.Info.Title;
                using var textWriter = new StringWriter(CultureInfo.InvariantCulture);
                var jsonWriter = new OpenApiJsonWriter(textWriter);
                document.SerializeAsV2(jsonWriter);
                var spec = textWriter.ToString();
                return (title, spec);
            });
        }
    }
}
