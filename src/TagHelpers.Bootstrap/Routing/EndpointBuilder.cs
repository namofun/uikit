using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// The <see cref="IEndpointRouteBuilder"/> wrapper for module design.
    /// </summary>
    public interface IEndpointBuilder
    {
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
        /// Adds a specialized <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that will match the provided pattern with the lowest possible priority.
        /// </summary>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="requestDelegate">The delegate executed when the endpoint is matched.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        IEndpointConventionBuilder MapFallback(string pattern, RequestDelegate requestDelegate);

        /// <summary>
        /// Adds a specialized <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that will match the provided pattern with the lowest possible priority.
        /// </summary>
        /// <param name="pattern">The route pattern.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        IEndpointConventionBuilder MapFallNotFound(string pattern);

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

        /// <summary>
        /// Maps incoming requests with the specified path to the specified <see cref="Hub"/> type.
        /// </summary>
        /// <typeparam name="THub">The <see cref="Hub"/> type to map requests to.</typeparam>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="configureOptions">A callback to configure dispatcher options.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        HubEndpointConventionBuilder MapHub<THub>(string pattern, Action<HttpConnectionDispatcherOptions>? configureOptions = null) where THub : Hub;

        /// <summary>
        /// Adds a specialized <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that will match the provided pattern with the lowest possible priority.
        /// </summary>
        /// <remarks>In this pattern, routes will explicitly end with no response body but a status code 404.</remarks>
        /// <param name="pattern">The route pattern.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public IEndpointConventionBuilder MapFallbackNotFound(string pattern)
        {
            return MapFallback(pattern, context =>
            {
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
            });
        }
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
}
