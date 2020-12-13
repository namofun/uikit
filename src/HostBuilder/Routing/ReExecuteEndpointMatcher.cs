using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    /// <summary>
    /// The matcher for status code pages re-execution.
    /// </summary>
    public class ReExecuteEndpointMatcher
    {
        private IReadOnlyList<(string RoutePattern, RoutePattern Pattern, ActionDescriptor ActionDescriptor)>? Fallbacks;
        private Func<HttpContext, Task>? PassTwo;
        private readonly IOptions<RouteOptions> _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<(string, RoutePattern, ActionDescriptor)> _fallbacks;

        /// <summary>
        /// Instantiate the <see cref="ReExecuteEndpointMatcher"/>.
        /// </summary>
        /// <param name="serviceProvider">The dependency injection container.</param>
        public ReExecuteEndpointMatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _options = _serviceProvider.GetRequiredService<IOptions<RouteOptions>>();
            _fallbacks = new List<(string, RoutePattern, ActionDescriptor)>();
        }

        /// <summary>
        /// Add the pattern and action descriptor to the fallback list.
        /// </summary>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <param name="pattern">The pattern string.</param>
        public void Add(string pattern, ActionDescriptor actionDescriptor)
        {
            if (Fallbacks != null)
                throw new InvalidOperationException("Patterns can't be added after finalizing endpoint building.");
            _fallbacks.Add((pattern, RoutePatternFactory.Parse(pattern), actionDescriptor));
        }

        /// <summary>
        /// Build the first pass of route binding.
        /// </summary>
        public void BuildPassOne()
        {
            Fallbacks = _fallbacks
                .OrderByDescending(a => a.Item2.PathSegments.Count)
                .ThenBy(a => a.Item1)
                .ToList();
        }

        /// <summary>
        /// Build the dynamic delegate for matching paths.
        /// </summary>
        public Func<HttpContext, Task> BuildPassTwo()
        {
            Debug.Assert(Fallbacks != null);
            var actionDescriptiors = Fallbacks
                .Select(a => a.ActionDescriptor)
                .Distinct()
                .ToHashSet();

            var endpoints = ((ICollection<EndpointDataSource>)typeof(RouteOptions)
                .GetProperty("EndpointDataSources", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(_options.Value)!)
                .SelectMany(a => a.Endpoints)
                .Where(a => actionDescriptiors.Contains(a.Metadata.GetMetadata<ActionDescriptor>()))
                .ToDictionary(a => a.Metadata.GetMetadata<ActionDescriptor>());

            var newEndpoints = new List<RouteEndpoint>();
            foreach (var (name, pattern, descriptor) in Fallbacks)
            {
                var oldEndpoint = endpoints[descriptor];
                newEndpoints.Add(new RouteEndpoint(
                    oldEndpoint.RequestDelegate,
                    pattern,
                    order: -pattern.PathSegments.Count,
                    oldEndpoint.Metadata,
                    displayName: $"Error Handler {name}"));
            }

            var dfaMatcherBuilderType = typeof(RouteEndpoint).Assembly
                .GetType("Microsoft.AspNetCore.Routing.Matching.DfaMatcherBuilder")!;
            var dfaMatcherType = typeof(RouteEndpoint).Assembly
                .GetType("Microsoft.AspNetCore.Routing.Matching.DfaMatcher")!;
            var dfaMatcherBuilder = _serviceProvider.GetRequiredService(dfaMatcherBuilderType);
            var addMethod = dfaMatcherBuilderType.GetMethod("AddEndpoint")!
                .CreateDelegate(typeof(Action<RouteEndpoint>), dfaMatcherBuilder)
                as Action<RouteEndpoint>;
            newEndpoints.ForEach(addMethod!);

            var matcher = dfaMatcherBuilderType.GetMethod("Build")!
                .Invoke(dfaMatcherBuilder, null);
            var matchMethod = dfaMatcherType.GetMethod("MatchAsync")!
                .CreateDelegate(typeof(Func<HttpContext, Task>), matcher)
                as Func<HttpContext, Task>;
            return matchMethod!;
        }

        /// <summary>
        /// Instantiate the dynamic delegate for matching paths with locks.
        /// </summary>
        private void EnsurePassTwo()
        {
            if (PassTwo == null)
            {
                lock (this)
                {
                    if (PassTwo == null)
                    {
                        PassTwo = BuildPassTwo();
                    }
                }
            }
        }

        /// <summary>
        /// Match whether such path exists.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <returns>The match task.</returns>
        public Task MatchAsync(HttpContext context)
        {
            EnsurePassTwo();
            Debug.Assert(PassTwo != null);
            return PassTwo(context);
        }
    }
}
