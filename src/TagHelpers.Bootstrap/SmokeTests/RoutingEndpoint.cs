using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.Diagnostics.SmokeTests
{
    internal class RoutingSmokeTest : SmokeTestBase<List<RoutingGroup>>
    {
        private readonly CompositeEndpointDataSource _compositeEndpointDataSource;
        private readonly ReExecuteEndpointDataSource _reExecuteEndpointDataSource;

        public RoutingSmokeTest(
            CompositeEndpointDataSource compositeEndpointDataSource,
            ReExecuteEndpointDataSource reExecuteEndpointDataSource)
        {
            _compositeEndpointDataSource = compositeEndpointDataSource;
            _reExecuteEndpointDataSource = reExecuteEndpointDataSource;
        }

        private static RoutingEndpoint FromRouteEndpoint(RouteEndpoint endpoint)
        {
            var actionDescriptor = endpoint.Metadata.GetMetadata<ActionDescriptor>();
            var inert = endpoint.Metadata.GetMetadata<ISuppressMatchingMetadata>()?.SuppressMatching ?? false;

            return new RoutingEndpoint
            {
                Area = (actionDescriptor?.RouteValues.ContainsKey("area") ?? false) ? actionDescriptor.RouteValues["area"] : null,
                AlternativeName = endpoint.Metadata.GetMetadata<IRouteNameMetadata>()?.RouteName,
                AllowedMethods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods?.ToArray() ?? Array.Empty<string>(),
                Inert = inert,
                NonMvc = actionDescriptor == null,
                Order = endpoint.Order,
                Description = endpoint.DisplayName,
                RoutePattern = '/' + endpoint.RoutePattern.RawText.TrimStart('/'),
                UsedForRouting = true,
            };
        }

        private static RoutingEndpoint FromEndpoint(Endpoint endpoint)
        {
            if (endpoint is RouteEndpoint routeEndpoint)
            {
                return FromRouteEndpoint(routeEndpoint);
            }
            else
            {
                return new RoutingEndpoint
                {
                    Description = endpoint.DisplayName,
                    UsedForRouting = false,
                };
            }
        }

        private static string GetExtendedName(Type type)
        {
            if (type.GenericTypeArguments.Length == 0)
            {
                return type.Name;
            }
            else
            {
                return type.Name[0..^2] +
                    "<" +
                    string.Join(',', type.GenericTypeArguments.Select(a => GetExtendedName(a))) +
                    ">";
            }
        }

        private static RoutingGroup FromEndpointDataSource(EndpointDataSource eds)
        {
            return new RoutingGroup
            {
                Name = GetExtendedName(eds.GetType()),
                Endpoints = eds.Endpoints.Select(FromEndpoint).ToList(),
            };
        }

        protected override List<RoutingGroup> Get()
        {
            var sources = _compositeEndpointDataSource.DataSources;
            if (_reExecuteEndpointDataSource.Endpoints.Count > 0)
            {
                sources = sources.Append(_reExecuteEndpointDataSource);
            }

            return sources.Select(FromEndpointDataSource).ToList();
        }
    }
}
