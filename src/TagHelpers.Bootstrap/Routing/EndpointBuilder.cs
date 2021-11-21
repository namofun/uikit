using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// The <see cref="IEndpointRouteBuilder"/> wrapper for module design.
    /// </summary>
    public interface IEndpointBuilder
    {
        /// <summary>
        /// Gets the endpoint data sources configured in the builder.
        /// </summary>
        ICollection<EndpointDataSource> DataSources { get; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> used to resolve services for routes.
        /// </summary>
        IServiceProvider ServiceProvider { get; }
        
        /// <summary>
        /// Gets the default conventions to apply to all the routes.
        /// </summary>
        Action<IEndpointConventionBuilder> DefaultConvention { get; }

        /// <summary>
        /// Creates a new <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <returns>The new <see cref="IApplicationBuilder"/>.</returns>
        IApplicationBuilder CreateApplicationBuilder();

        /// <summary>
        /// Adds endpoints for controller actions to the <see cref="IEndpointRouteBuilder"/>
        /// without specifying any routes.
        /// </summary>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        ControllerActionEndpointConventionBuilder MapControllers();

        /// <summary>
        /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that matches HTTP requests for the specified pattern.
        /// </summary>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="requestDelegate">The delegate executed when the endpoint is matched.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        IEndpointConventionBuilder MapRequestDelegate(string pattern, RequestDelegate requestDelegate);

        /// <summary>
        /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that matches HTTP requests for the specified pattern.
        /// </summary>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="requestDelegate">The delegate executed when the endpoint is matched.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        IEndpointConventionBuilder MapRequestDelegate(RoutePattern pattern, RequestDelegate requestDelegate);

        /// <summary>
        /// Adds a specialized <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that will match the provided pattern with the lowest possible priority.
        /// </summary>
        /// <remarks>
        /// <see cref="MapFallback(string, RequestDelegate)"/> is intended to handle cases where no
        /// other endpoint has matched. This is convenient for routing requests to a SPA framework.
        /// </remarks>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="requestDelegate">The delegate executed when the endpoint is matched.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        IEndpointConventionBuilder MapFallback(string pattern, RequestDelegate requestDelegate);

        /// <summary>
        /// Adds a specialized <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that will match the provided pattern with the lowest possible priority.
        /// </summary>
        /// <remarks>
        /// <see cref="MapFallback(RoutePattern, RequestDelegate)"/> is intended to handle cases where no
        /// other endpoint has matched. This is convenient for routing requests to a SPA framework.
        /// </remarks>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="requestDelegate">The delegate executed when the endpoint is matched.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        IEndpointConventionBuilder MapFallback(RoutePattern pattern, RequestDelegate requestDelegate);

        /// <summary>
        /// Adds a specialized <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that will match the provided pattern with the lowest possible priority.
        /// </summary>
        /// <remarks>In this pattern, routes will explicitly end with no response body but a status code 404.</remarks>
        /// <param name="pattern">The route pattern.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public IEndpointConventionBuilder MapFallNotFound(string pattern)
        {
            return MapFallback(pattern, context =>
            {
                context.Features.Get<IStatusCodePagesFeature>()!.Enabled = false;
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
            })
            .WithDisplayName("Empty Response " + pattern + " (Fallback NotFound)");
        }

        /// <summary>
        /// Adds a specialized <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that will match the provided pattern with the lowest possible priority.
        /// </summary>
        /// <remarks>
        /// If such error handler is set up, the routes registered by
        /// <see cref="IErrorHandlerBuilder.MapFallbackNotFound(string)" /> and
        /// <see cref="IErrorHandlerBuilder.MapStatusCode(string)" />
        /// will be handled with this handler.
        /// </remarks>
        /// <param name="area">The name of area.</param>
        /// <param name="controller">The name of controller.</param>
        /// <param name="action">The name of action.</param>
        /// <returns>A <see cref="IErrorHandlerBuilder"/> that can be used to further customize the error handler.</returns>
        IErrorHandlerBuilder WithErrorHandler(string area, string controller, string action = nameof(ViewControllerBase.StatusCodePage));

        /// <summary>
        /// Adds a specialized <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that will provide a swagger document UI on <c>/api/doc/{<paramref name="name"/>}</c>.
        /// </summary>
        /// <param name="name">The name of API document.</param>
        /// <param name="title">The title of API.</param>
        /// <param name="description">The description of API.</param>
        /// <param name="version">The version of API.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        IEndpointConventionBuilder MapApiDocument(string name, string title, string description, string version);
    }

    /// <summary>
    /// The <see cref="IEndpointRouteBuilder"/> wrapper for module error handler.
    /// </summary>
    public interface IErrorHandlerBuilder
    {
        /// <summary>
        /// Adds a specialized <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that will match the provided pattern with the lowest possible priority.
        /// </summary>
        /// <remarks>In this pattern, the error handler will be routed when url is not routed.</remarks>
        /// <param name="pattern">The route pattern.</param>
        /// <returns>A <see cref="IErrorHandlerBuilder"/> that can be used to further customize the error handler.</returns>
        IErrorHandlerBuilder MapFallbackNotFound(string pattern);

        /// <summary>
        /// Adds a specialized <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that will match the provided pattern with the lowest possible priority.
        /// </summary>
        /// <remarks>In this pattern, the error handler will be routed when previous execution of endpoint is not success.</remarks>
        /// <param name="pattern">The route pattern.</param>
        /// <returns>A <see cref="IErrorHandlerBuilder"/> that can be used to further customize the error handler.</returns>
        IErrorHandlerBuilder MapStatusCode(string pattern);
    }

    /// <summary>
    /// Class for endpoint builder conventions related.
    /// </summary>
    public static class EndpointBuilderConventionExtensions
    {
        /// <summary>
        /// Conventions for setting up order for route endpoints.
        /// </summary>
        /// <typeparam name="TEndpointConventionBuilder">The endpoint convention builder.</typeparam>
        /// <param name="builder">The endpoint convention builder.</param>
        /// <param name="order">The order to set.</param>
        /// <returns>The endpoint convention builder to chain the configurations.</returns>
        public static TEndpointConventionBuilder WithOrder<TEndpointConventionBuilder>(this TEndpointConventionBuilder builder, int order) where TEndpointConventionBuilder : IEndpointConventionBuilder
        {
            builder.Add(builder =>
            {
                if (builder is RouteEndpointBuilder routeEndpointBuilder)
                {
                    routeEndpointBuilder.Order = order;
                }
            });

            return builder;
        }
    }
}
