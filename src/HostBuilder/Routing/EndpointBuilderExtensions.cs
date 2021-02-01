using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Menus;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// The extension methods for substrate endpoint route builder.
    /// </summary>
    public static class SubstrateEndpointBuilderExtensions
    {
        /// <summary>
        /// Internally casts the <see cref="IEndpointBuilder"/> to <see cref="IEndpointRouteBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointBuilder"/>.</param>
        /// <returns>The <see cref="IEndpointRouteBuilder"/>.</returns>
        private static IEndpointRouteBuilder InternalCast(IEndpointBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (builder is IEndpointRouteBuilder inner)
                return inner;

            throw new InvalidCastException("This instance of IEndpointBuilder cannot be cast to IEndpointRouteBuilder.");
        }

        /// <summary>
        /// Maps incoming requests with the specified path to the specified <see cref="Hub"/> type.
        /// </summary>
        /// <typeparam name="THub">The <see cref="Hub"/> type to map requests to.</typeparam>
        /// <param name="builder">The endpoint route builder.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="configureOptions">A callback to configure dispatcher options.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public static HubEndpointConventionBuilder MapHub<THub>(
            this IEndpointBuilder builder,
            string pattern,
            Action<HttpConnectionDispatcherOptions>? configureOptions = null)
            where THub : Hub
        {
            return InternalCast(builder)
                .MapHub<THub>(pattern, configureOptions)
                .WithDefaults(builder.DefaultConvention);
        }

        /// <summary>
        /// Maps the Blazor <see cref="Hub"/> to the default path.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointBuilder"/>.</param>
        /// <param name="configureOptions">A callback to configure dispatcher options.</param>
        /// <returns>The <see cref="ComponentEndpointConventionBuilder"/>.</returns>
        public static ComponentEndpointConventionBuilder MapBlazorHub(
            this IEndpointBuilder builder,
            Action<HttpConnectionDispatcherOptions>? configureOptions = null)
        {
            return InternalCast(builder)
                .MapBlazorHub(configureOptions)
                .WithDefaults(builder.DefaultConvention);
        }

        /// <summary>
        /// Maps the Blazor <see cref="Hub"/> to the path <paramref name="path"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointBuilder"/>.</param>
        /// <param name="path">The path to map the Blazor <see cref="Hub"/>.</param>
        /// <param name="configureOptions">A callback to configure dispatcher options.</param>
        /// <returns>The <see cref="ComponentEndpointConventionBuilder"/>.</returns>
        public static ComponentEndpointConventionBuilder MapBlazorHub(
            this IEndpointBuilder builder, string path,
            Action<HttpConnectionDispatcherOptions> configureOptions)
        {
            return InternalCast(builder)
                .MapBlazorHub(path, configureOptions)
                .WithDefaults(builder.DefaultConvention);
        }

        /// <summary>
        /// Maps incoming requests with the specified path to the provided connection pipeline.
        /// </summary>
        /// <typeparam name="TConnectionHandler">The <see cref="ConnectionHandler"/> type.</typeparam>
        /// <param name="builder">The <see cref="IEndpointBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="configureOptions">A callback to configure dispatcher options.</param>
        /// <returns>An <see cref="ConnectionEndpointRouteBuilder"/> for endpoints associated with the connections.</returns>
        public static ConnectionEndpointRouteBuilder MapConnectionHandler<TConnectionHandler>(
            this IEndpointBuilder builder, string pattern,
            Action<HttpConnectionDispatcherOptions>? configureOptions = null)
            where TConnectionHandler : ConnectionHandler
        {
            return InternalCast(builder)
                .MapConnectionHandler<TConnectionHandler>(pattern, configureOptions)
                .WithDefaults(builder.DefaultConvention);
        }

        /// <summary>
        /// Maps incoming requests with the specified path to the provided connection pipeline.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="options">Options used to configure the connection.</param>
        /// <param name="configure">A callback to configure the connection.</param>
        /// <returns>An <see cref="ConnectionEndpointRouteBuilder"/> for endpoints associated with the connections.</returns>
        public static ConnectionEndpointRouteBuilder MapConnections(
            this IEndpointBuilder builder, string pattern,
            HttpConnectionDispatcherOptions options,
            Action<IConnectionBuilder> configure)
        {
            return InternalCast(builder)
                .MapConnections(pattern, options, configure)
                .WithDefaults(builder.DefaultConvention);
        }

        /// <summary>
        /// Maps incoming requests with the specified path to the provided connection pipeline.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="configure">A callback to configure the connection.</param>
        /// <returns>An <see cref="ConnectionEndpointRouteBuilder"/> for endpoints associated with the connections.</returns>
        public static ConnectionEndpointRouteBuilder MapConnections(
            this IEndpointBuilder builder, string pattern,
            Action<IConnectionBuilder> configure)
        {
            return InternalCast(builder)
                .MapConnections(pattern, configure)
                .WithDefaults(builder.DefaultConvention);
        }

        /// <summary>
        /// Adds a health checks endpoint to the <see cref="IEndpointBuilder"/> with the specified template and options.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointBuilder"/> to add the health checks endpoint to.</param>
        /// <param name="pattern">The URL pattern of the health checks endpoint.</param>
        /// <param name="options">A <see cref="HealthCheckOptions"/> used to configure the health checks.</param>
        /// <returns>A convention routes for the health checks endpoint.</returns>
        public static IEndpointConventionBuilder MapHealthChecks(
            this IEndpointBuilder builder, string pattern,
            HealthCheckOptions? options = null)
        {
            if (builder.ServiceProvider.GetService(typeof(HealthCheckService)) == null)
                throw new InvalidOperationException("Unable to find services. Please service.AddHealthChecks() in ConfigureServices(...).");

            var args = options != null ? new[] { Options.Create(options) } : Array.Empty<object>();

            var pipeline = builder.CreateApplicationBuilder()
               .UseMiddleware<HealthCheckMiddleware>(args)
               .Build();

            return builder.MapRequestDelegate(pattern, pipeline)
                .WithDisplayName("Health checks");
        }

        /// <summary>
        /// Apply the endpoint modules into the route builder.
        /// </summary>
        /// <param name="builder">The route builder</param>
        /// <param name="modules">The endpoint configuration list</param>
        /// <returns>The route builder</returns>
        internal static void MapModules(this IEndpointRouteBuilder builder, IReadOnlyCollection<AbstractModule> modules)
        {
            var menu = builder.ServiceProvider.GetRequiredService<ConcreteMenuContributor>();
            foreach (var module in modules)
            {
                module.RegisterEndpoints(ModuleEndpointDataSource.Create(module, builder));
                module.RegisterMenu(menu);
            }

            menu.Contribute();
        }

        /// <summary>
        /// Apply the re-execute endpoint into the route builder.
        /// </summary>
        /// <param name="builder">The route builder</param>
        /// <returns>The route builder</returns>
        internal static void MapReExecute(this IEndpointRouteBuilder builder)
        {
            builder.ServiceProvider
                .GetRequiredService<ReExecuteEndpointDataSource>()
                .Discover();
        }

        /// <summary>
        /// Apply the extensions into the route builder.
        /// </summary>
        /// <param name="builder">The route builder</param>
        /// <param name="actions">The extensions points</param>
        /// <returns>The route builder</returns>
        internal static void MapExtensions(this IEndpointRouteBuilder builder, List<Action<IEndpointRouteBuilder>> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].Invoke(builder);
            }
        }

        /// <summary>
        /// Conventions for endpoints that requires the authorization.
        /// </summary>
        /// <param name="builder">The endpoint convention builder.</param>
        /// <param name="roles">The roles to be confirmed.</param>
        /// <returns>The endpoint convention builder to chain the configurations.</returns>
        public static TEndpointConventionBuilder RequireRoles<TEndpointConventionBuilder>(this TEndpointConventionBuilder builder, string roles) where TEndpointConventionBuilder : IEndpointConventionBuilder
        {
            return builder.RequireAuthorization(new AuthorizeAttribute { Roles = roles });
        }

        /// <summary>
        /// Conventions in default.
        /// </summary>
        /// <param name="builder">The endpoint convention builder.</param>
        /// <param name="configure">The configure delegate.</param>
        /// <returns>The endpoint convention builder to chain the configurations.</returns>
        internal static TEndpointConventionBuilder WithDefaults<TEndpointConventionBuilder>(this TEndpointConventionBuilder builder, Action<IEndpointConventionBuilder> configure) where TEndpointConventionBuilder : IEndpointConventionBuilder
        {
            configure.Invoke(builder);
            return builder;
        }
    }
}
