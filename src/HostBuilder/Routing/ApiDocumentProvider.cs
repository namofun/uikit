using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Globalization;
using System.IO;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class ApiDocumentProvider : IApiDocumentProvider
    {
        private readonly IMemoryCache _apiDocCache;
        private readonly ISwaggerProvider _swaggerGen;

        public ApiDocumentProvider(ISwaggerProvider swaggerGen, IMemoryCache memoryCache)
        {
            _swaggerGen = swaggerGen;
            _apiDocCache = memoryCache;
        }

        public IApiDocument GetDocument(string name)
        {
            return _apiDocCache.GetOrCreate(name, entry =>
            {
                OpenApiDocument document = _swaggerGen.GetSwagger(name);
                return new SwaggerDocument(name, document);
            });
        }

        private class ApiDocumentCacheKey : IEquatable<ApiDocumentCacheKey>
        {
            private readonly string _key;

            public ApiDocumentCacheKey(string key)
            {
                this._key = key;
            }

            public bool Equals(ApiDocumentCacheKey? other)
            {
                return other != null && _key == other._key;
            }

            public override bool Equals(object? obj)
            {
                return Equals(obj as ApiDocumentCacheKey);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(248947, _key);
            }

            public override string ToString()
            {
                return "ApiDocument " + _key;
            }
        }

        private class SwaggerDocument : IApiDocument
        {
            private string? _swaggerV2Json, _swaggerV3Json;
            private string? _swaggerV2Yaml, _swaggerV3Yaml;

            public SwaggerDocument(string name, OpenApiDocument document)
            {
                Name = name;
                Document = document;
            }

            public string Name { get; }

            public string Title => Document.Info.Title;

            public OpenApiDocument Document { get; }

            public string GetSpecification(ApiSpecificationType type)
            {
                return type switch
                {
                    ApiSpecificationType.SwaggerV2Json => _swaggerV2Json ??= SerializeOpenApi((d, w) => d.SerializeAsV2(new OpenApiJsonWriter(w))),
                    ApiSpecificationType.SwaggerV3Json => _swaggerV3Json ??= SerializeOpenApi((d, w) => d.SerializeAsV3(new OpenApiJsonWriter(w))),
                    ApiSpecificationType.SwaggerV2Yaml => _swaggerV2Yaml ??= SerializeOpenApi((d, w) => d.SerializeAsV2(new OpenApiYamlWriter(w))),
                    ApiSpecificationType.SwaggerV3Yaml => _swaggerV3Yaml ??= SerializeOpenApi((d, w) => d.SerializeAsV3(new OpenApiYamlWriter(w))),
                    _ => throw new NotSupportedException("Currently doesn't support " + type + " type."),
                };
            }

            private string SerializeOpenApi(Action<OpenApiDocument, StringWriter> serialize)
            {
                using StringWriter textWriter = new(CultureInfo.InvariantCulture);
                serialize(Document, textWriter);
                return textWriter.ToString();
            }
        }
    }
}
