using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Routing
{
    internal class DefaultEndpointConventionBuilder : IEndpointConventionBuilder
    {
        private const int defaultOrder = 0;
        private readonly int _order = defaultOrder;
        private readonly RoutePattern _routePattern;
        private readonly string _displayName;
        private readonly IReadOnlyList<object> _metadata;
        private readonly RequestDelegate _requestDelegate;
        private readonly List<Action<EndpointBuilder>> _conventions;

        public DefaultEndpointConventionBuilder(RoutePattern routePattern, RequestDelegate requestDelegate)
        {
            _conventions = new List<Action<EndpointBuilder>>();

            _routePattern = routePattern;
            _displayName = routePattern.RawText ?? "null";
            _requestDelegate = requestDelegate;

            // Add delegate attributes as metadata
            // This can be null if the delegate is a dynamic method or compiled from an expression tree
            _metadata = new List<object>(requestDelegate.Method?.GetCustomAttributes() ?? Enumerable.Empty<Attribute>());
        }

        public void Add(Action<EndpointBuilder> convention)
        {
            _conventions.Add(convention);
        }

        public Endpoint Build()
        {
            var endpointBuilder = new RouteEndpointBuilder(_requestDelegate, _routePattern, _order)
            {
                DisplayName = _displayName,
            };

            foreach (var oldMetadata in _metadata)
            {
                endpointBuilder.Metadata.Add(oldMetadata);
            }

            foreach (var convention in _conventions)
            {
                convention(endpointBuilder);
            }

            return endpointBuilder.Build();
        }
    }
}
