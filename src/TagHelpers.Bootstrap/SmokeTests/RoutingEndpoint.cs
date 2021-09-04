using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.Diagnostics.SmokeTests
{
    /// <summary>
    /// The smoke test target of routing endpoints.
    /// </summary>
    public class RoutingEndpoint
    {
        /// <summary>
        /// The MVC area of endpoint
        /// </summary>
        public string? Area { get; set; }

        /// <summary>
        /// Whether this is MVC endpoint
        /// </summary>
        public bool NonMvc { get; set; }

        /// <summary>
        /// Whether this endpoint is used in matching
        /// </summary>
        public bool Inert { get; set; }

        /// <summary>
        /// The alternative endpoint name
        /// </summary>
        public string? AlternativeName { get; set; }

        /// <summary>
        /// The order of endpoint
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// The allowed methods for HTTP requests
        /// </summary>
        public string[] AllowedMethods { get; set; } = Array.Empty<string>();

        /// <summary>
        /// The routing pattern
        /// </summary>
        public string? RoutePattern { get; set; }

        /// <summary>
        /// The endpoint description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Whether this endpoint is used for routing
        /// </summary>
        public bool UsedForRouting { get; set; }

        /// <summary>
        /// Initialize a routing endpoint with the corresponding endpoint.
        /// </summary>
        /// <param name="endpoint">The route endpoint.</param>
        /// <returns>The metadata of endpoint.</returns>
        public static RoutingEndpoint FromRouteEndpoint(RouteEndpoint endpoint)
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

        /// <summary>
        /// Initialize a routing endpoint with the corresponding endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns>The metadata of endpoint.</returns>
        public static RoutingEndpoint FromEndpoint(Endpoint endpoint)
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
    }

    /// <summary>
    /// The group of routing endpoints.
    /// </summary>
    public class RoutingGroup
    {
        /// <summary>
        /// The routing group name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The endpoint list
        /// </summary>
        public List<RoutingEndpoint> Endpoints { get; set; }

        /// <summary>
        /// Initialize the routing group.
        /// </summary>
#pragma warning disable CS8618
        public RoutingGroup()
#pragma warning restore CS8618
        {
        }

        /// <summary>
        /// Gets the extended generic name of type.
        /// </summary>
        /// <param name="type">The type to get.</param>
        /// <returns>The type name to display.</returns>
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

        /// <summary>
        /// Gets a routing group from endpoint data source.
        /// </summary>
        /// <param name="eds">The endpoint data source.</param>
        /// <returns>The routing group metadata.</returns>
        public static RoutingGroup FromEndpointDataSource(EndpointDataSource eds)
        {
            return new RoutingGroup
            {
                Name = GetExtendedName(eds.GetType()),
                Endpoints = eds.Endpoints.Select(RoutingEndpoint.FromEndpoint).ToList(),
            };
        }
    }

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

        protected override List<RoutingGroup> Get()
        {
            var sources = _compositeEndpointDataSource.DataSources;
            if (_reExecuteEndpointDataSource.Endpoints.Count > 0)
            {
                sources = sources.Append(_reExecuteEndpointDataSource);
            }

            return sources.Select(RoutingGroup.FromEndpointDataSource).ToList();
        }
    }
}
