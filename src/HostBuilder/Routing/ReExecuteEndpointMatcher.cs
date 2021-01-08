using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    /// <summary>
    /// The matcher for status code pages re-execution.
    /// </summary>
    public class ReExecuteEndpointMatcher
    {
        private Func<HttpContext, Task>? PassTwo;
        private readonly IServiceProvider _serviceProvider;
        private readonly ReExecuteEndpointDataSource _endpointDataSource;

        /// <summary>
        /// Instantiate the <see cref="ReExecuteEndpointMatcher"/>.
        /// </summary>
        /// <param name="serviceProvider">The dependency injection container.</param>
        public ReExecuteEndpointMatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _endpointDataSource = _serviceProvider.GetRequiredService<ReExecuteEndpointDataSource>();
        }

        /// <summary>
        /// Build the dynamic delegate for matching paths.
        /// </summary>
        public Func<HttpContext, Task> BuildPassTwo()
        {
            var dfaMatcherBuilderType = typeof(RouteEndpoint).Assembly
                .GetType("Microsoft.AspNetCore.Routing.Matching.DfaMatcherBuilder")!;
            var dfaMatcherType = typeof(RouteEndpoint).Assembly
                .GetType("Microsoft.AspNetCore.Routing.Matching.DfaMatcher")!;
            var dfaMatcherBuilder = _serviceProvider.GetRequiredService(dfaMatcherBuilderType);
            var addMethod = dfaMatcherBuilderType.GetMethod("AddEndpoint")!
                .CreateDelegate(typeof(Action<RouteEndpoint>), dfaMatcherBuilder)
                as Action<RouteEndpoint>;

            foreach (var endpoint in _endpointDataSource.Endpoints)
                addMethod!.Invoke((RouteEndpoint)endpoint);

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
