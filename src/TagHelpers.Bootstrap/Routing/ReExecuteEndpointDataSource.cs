using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.AspNetCore.Routing
{
    /// <summary>
    /// The <see cref="EndpointDataSource"/> for re-execute endpoints.
    /// This data source is independent from the global <see cref="CompositeEndpointDataSource"/>.
    /// </summary>
    public class ReExecuteEndpointDataSource : EndpointDataSource
    {
        private static readonly object _locker = new object();
        private readonly List<(string, RoutePattern, ControllerActionDescriptorWrapper)> _fallbacks;
        private readonly IServiceProvider _serviceProvider;
        private readonly CompositeEndpointDataSource _compositeEndpointDataSource;
        private IReadOnlyList<(string RoutePattern, RoutePattern Pattern, ActionDescriptor ActionDescriptor)>? _discoveredFallbacks;
        private IReadOnlyList<RouteEndpoint>? _endpoints;

        /// <summary>
        /// Instantiate the <see cref="ReExecuteEndpointDataSource"/>.
        /// </summary>
        /// <param name="serviceProvider">The dependency injection container.</param>
        public ReExecuteEndpointDataSource(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _fallbacks = new List<(string, RoutePattern, ControllerActionDescriptorWrapper)>();
            _compositeEndpointDataSource = _serviceProvider.GetRequiredService<CompositeEndpointDataSource>();
        }

        /// <summary>
        /// Add the pattern and action descriptor to the fallback list.
        /// </summary>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <param name="pattern">The pattern string.</param>
        public void Add(string pattern, ControllerActionDescriptorWrapper actionDescriptor)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));
            if (actionDescriptor == null)
                throw new ArgumentNullException(nameof(actionDescriptor));
            if (_discoveredFallbacks != null)
                throw new InvalidOperationException("Patterns can't be added after finalizing endpoint building.");

            _fallbacks.Add((pattern, actionDescriptor.GetPattern(pattern), actionDescriptor));
        }

        /// <summary>
        /// Build the first pass of route binding.
        /// </summary>
        public void Discover()
        {
            _discoveredFallbacks = _fallbacks
                .OrderByDescending(a => a.Item2.PathSegments.Count)
                .ThenBy(a => a.Item1)
                .Select(a => (a.Item1, a.Item2, (ActionDescriptor)a.Item3.GetValue(_serviceProvider)))
                .ToList();
        }

        /// <summary>
        /// Creates the endpoints from discovered endpoints.
        /// </summary>
        /// <returns>The list of <see cref="RouteEndpoint"/>s.</returns>
        private List<RouteEndpoint> CreateEndpointsCore()
        {
            if (_discoveredFallbacks == null)
                throw new InvalidOperationException("The re-execute endpoints was requested to be got before finishing discover.");

            var actionDescriptiors = _discoveredFallbacks
                .Select(a => a.ActionDescriptor)
                .Distinct()
                .ToHashSet();

            var endpoints = _compositeEndpointDataSource.Endpoints
                .Select(a => (ep: a, ad: a.Metadata.GetMetadata<ActionDescriptor>()))
                .Where(a => a.ad != null && actionDescriptiors.Contains(a.ad))
                .GroupBy(a => a.ad!, a => a.ep)
                .ToDictionary(a => a.Key, a => a.First());

            var newEndpoints = new List<RouteEndpoint>();
            foreach (var (name, pattern, descriptor) in _discoveredFallbacks)
            {
                if (!endpoints.TryGetValue(descriptor, out var oldEndpoint))
                    throw new InvalidOperationException(
                        "The endpoint to re-execute is not found in original builder.");

                var metadata = oldEndpoint.Metadata
                    .Where(a => !(a is ISuppressMatchingMetadata))
                    .Append(TrackAvailabilityMetadata.ErrorHandler);

                newEndpoints.Add(new RouteEndpoint(
                    oldEndpoint.RequestDelegate ?? throw new InvalidOperationException("No request delegate for original endpoint."),
                    pattern,
                    order: -pattern.PathSegments.Count,
                    new EndpointMetadataCollection(metadata),
                    displayName: $"Error Handler {name}"));
            }

            return newEndpoints;
        }

        /// <summary>
        /// Ensure the endpoints are initialized.
        /// </summary>
        private void Initialize()
        {
            if (_endpoints == null)
            {
                lock (_locker)
                {
                    if (_endpoints == null)
                    {
                        _endpoints = CreateEndpointsCore();
                    }
                }
            }
        }

        /// <inheritdoc />
        public override IReadOnlyList<Endpoint> Endpoints
        {
            get
            {
                Initialize();
                Debug.Assert(_endpoints != null);
                return _endpoints;
            }
        }

        /// <inheritdoc />
        public override IChangeToken GetChangeToken()
        {
            return NullChangeToken.Singleton;
        }
    }
}
