using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Support moduling by extension methods on <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    public static class ModuleEndpointBuilderExtensions
    {
        /// <summary>
        /// Adds a specialized <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that will match the provided pattern with the lowest possible priority.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/></param>
        /// <param name="pattern">The url pattern (useful <c>{**slug}</c>)</param>
        /// <returns>The <see cref="IEndpointConventionBuilder"/></returns>
        public static IEndpointConventionBuilder MapFallbackNotFound(this IEndpointRouteBuilder endpoints, string pattern)
        {
            return endpoints.MapFallback(pattern, context =>
            {
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
            });
        }
    }
}
