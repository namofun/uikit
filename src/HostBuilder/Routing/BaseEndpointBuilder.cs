using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing
{
    internal abstract class BaseEndpointBuilder : IEndpointBuilder, IEndpointRouteBuilder
    {
        private readonly IEndpointRouteBuilder _innerBuilder;

        public ICollection<EndpointDataSource> DataSources => _innerBuilder.DataSources;

        public IServiceProvider ServiceProvider => _innerBuilder.ServiceProvider;

        public Action<IEndpointConventionBuilder> DefaultConvention { get; }

        public string AreaName { get; }

        public BaseEndpointBuilder(IEndpointRouteBuilder builder, string areaName, Action<IEndpointConventionBuilder> convention)
        {
            AreaName = areaName;
            _innerBuilder = builder;
            DefaultConvention = convention;
        }

        protected abstract ModuleEndpointDataSource GetOrCreateDataSource();

        public abstract IEndpointConventionBuilder MapApiDocument(string name, string title, string description, string version);

        public abstract ControllerActionEndpointConventionBuilder MapControllers();

        public IErrorHandlerBuilder WithErrorHandler(string area, string controller, string action)
        {
            var ad = new ControllerActionDescriptorWrapper(area, controller, action);
            return new DefaultErrorHandlerBuilder(ad, this);
        }

        public IEndpointConventionBuilder MapFallback(string pattern, RequestDelegate requestDelegate)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            return MapFallback(RoutePatternFactory.Parse(pattern), requestDelegate);
        }

        public IEndpointConventionBuilder MapFallback(RoutePattern pattern, RequestDelegate requestDelegate)
        {
            return MapRequestDelegate(pattern, requestDelegate)
                .WithDisplayName("Fallback " + pattern)
                .WithMetadata(TrackAvailabilityMetadata.Fallback)
                .WithDefaults(a => a.Add(b => ((RouteEndpointBuilder)b).Order = int.MaxValue));
        }

        public IEndpointConventionBuilder MapRequestDelegate(string pattern, RequestDelegate requestDelegate)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            return MapRequestDelegate(RoutePatternFactory.Parse(pattern), requestDelegate);
        }

        public IEndpointConventionBuilder MapRequestDelegate(RoutePattern routePattern, RequestDelegate requestDelegate)
        {
            if (routePattern == null)
            {
                throw new ArgumentNullException(nameof(routePattern));
            }

            if (requestDelegate == null)
            {
                throw new ArgumentNullException(nameof(requestDelegate));
            }

            return GetOrCreateDataSource()
                .AddRequestDelegate(routePattern, requestDelegate)
                .WithDefaults(DefaultConvention);
        }

        public IApplicationBuilder CreateApplicationBuilder()
        {
            return _innerBuilder.CreateApplicationBuilder();
        }
    }
}
