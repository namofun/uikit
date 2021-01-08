using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Routing
{
    internal class DefaultEndpointBuilder<TModule> : IEndpointBuilder, IEndpointRouteBuilder
        where TModule : AbstractModule
    {
        private readonly IEndpointRouteBuilder _innerBuilder;

        public ICollection<EndpointDataSource> DataSources => _innerBuilder.DataSources;

        public IServiceProvider ServiceProvider => _innerBuilder.ServiceProvider;

        public Action<IEndpointConventionBuilder> DefaultConvention { get; }

        public string AreaName { get; }

        public DefaultEndpointBuilder(IEndpointRouteBuilder builder, string areaName, Action<IEndpointConventionBuilder> convention)
        {
            AreaName = areaName;
            _innerBuilder = builder;
            DefaultConvention = convention;
        }

        private ModuleEndpointDataSource<TModule> GetOrCreateDataSource()
        {
            var dataSource = DataSources.OfType<ModuleEndpointDataSource<TModule>>().FirstOrDefault();
            if (dataSource == null)
            {
                dataSource = ServiceProvider.GetRequiredService<ModuleEndpointDataSource<TModule>>();
                DataSources.Add(dataSource);
            }

            return dataSource;
        }

        public IEndpointConventionBuilder MapApiDocument(string name, string title, string description, string version)
        {
            var assembly = typeof(TModule).Assembly;
            ServiceProvider
                .GetRequiredService<SubstrateControllerConvention>()
                .Declare(assembly.FullName!, name);

            var sgo = ServiceProvider.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

            sgo.SwaggerDoc(name,
                new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = title,
                    Description = description,
                    Version = version,
                });

            var file = System.IO.Path.ChangeExtension(assembly.Location, "xml");
            if (System.IO.File.Exists(file))
            {
                sgo.IncludeXmlComments(file);
            }
            else
            {
                ServiceProvider
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Microsoft.Hosting.Lifetime")
                    .LogWarning($"Documentation '{file}' is not found. Specification comments will not be registered into swagger.");
            }

            var actionLazy = new ControllerActionDescriptorWrapper("Dashboard", "ApiDoc", "Display");

            return MapRequestDelegate(
                "/api/doc/" + name, context =>
                {
                    var routeData = new RouteData();
                    routeData.PushState(router: null, context.Request.RouteValues, new RouteValueDictionary());
                    routeData.Values["name"] = name;
                    var actionContext = new ActionContext(context, routeData, actionLazy.GetValue(context.RequestServices));

                    var invoker = context.RequestServices
                        .GetRequiredService<IActionInvokerFactory>()
                        .CreateInvoker(actionContext);

                    return invoker.InvokeAsync();
                })
                .WithDisplayName($"Swagger Document ({name})")
                .WithMetadata(new HttpMethodMetadata(new[] { "GET" }));
        }

        public ControllerActionEndpointConventionBuilder MapControllers()
        {
            var endpointDataSorce = GetOrCreateDataSource();
            if (endpointDataSorce.EnableController)
            {
                return endpointDataSorce.ConventionBuilder;
            }
            else
            {
                endpointDataSorce.EnableController = true;
                return endpointDataSorce.ConventionBuilder
                    .WithDefaults(DefaultConvention)
                    .WithDisplayName(TransformDisplayName);

                static string TransformDisplayName(EndpointBuilder builder)
                {
                    var original = builder.DisplayName;
                    var segments = original.Split(' ');
                    if (segments.Length != 2) return original;
                    if (!segments[1].StartsWith('(') || !segments[1].EndsWith(')')) return original;
                    var prefix = segments[1][1..^1] + ".";
                    if (original.StartsWith(prefix)) return original[prefix.Length..];
                    return original;
                }
            }
        }

        public IErrorHandlerBuilder WithErrorHandler(string area, string controller, string action)
        {
            var ad = new ControllerActionDescriptorWrapper(area, controller, action);
            return new DefaultErrorHandlerBuilder(ad, this);
        }

        public IEndpointConventionBuilder MapFallback(string pattern, RequestDelegate requestDelegate)
        {
            return MapRequestDelegate(pattern, requestDelegate)
                .WithDisplayName("Fallback " + pattern)
                .WithDefaults(a => a.Add(b => ((RouteEndpointBuilder)b).Order = int.MaxValue));
        }

        public IEndpointConventionBuilder MapRequestDelegate(string pattern, RequestDelegate requestDelegate)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));
            if (requestDelegate == null)
                throw new ArgumentNullException(nameof(requestDelegate));
            var routePattern = RoutePatternFactory.Parse(pattern);

            const int defaultOrder = 0;

            var builder = new RouteEndpointBuilder(
                requestDelegate, routePattern, defaultOrder)
            {
                DisplayName = routePattern.RawText ?? "null",
            };

            // Add delegate attributes as metadata
            var attributes = requestDelegate.Method.GetCustomAttributes();

            // This can be null if the delegate is a dynamic method or compiled from an expression tree
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    builder.Metadata.Add(attribute);
                }
            }

            var dataSource = GetOrCreateDataSource();
            return dataSource.AddEndpointBuilder(builder).WithDefaults(DefaultConvention);
        }

        public IApplicationBuilder CreateApplicationBuilder()
        {
            return _innerBuilder.CreateApplicationBuilder();
        }
    }
}
