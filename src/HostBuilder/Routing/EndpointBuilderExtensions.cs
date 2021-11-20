using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// The extension methods for connection endpoint route builder.
    /// </summary>
    public static class ConnectionEndpointBuilderExtensions
    {
        private const string _httpConnectionDispatcherTypeName = "Microsoft.AspNetCore.Http.Connections.Internal.HttpConnectionDispatcher";
        private static readonly Type _httpConnectionDispatcherType
            = typeof(HttpConnectionDispatcherOptions).Assembly.GetType(_httpConnectionDispatcherTypeName)
            ?? throw new TypeLoadException("No " + _httpConnectionDispatcherTypeName + " found.");

        /// <summary>
        /// Maps incoming requests with the specified path to the provided connection pipeline.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="configure">A callback to configure the connection.</param>
        /// <returns>An <see cref="ConnectionEndpointRouteBuilder"/> for endpoints associated with the connections.</returns>
        public static ConnectionEndpointRouteBuilder MapConnections(this IEndpointBuilder endpoints, string pattern, Action<IConnectionBuilder> configure) =>
            endpoints.MapConnections(pattern, new HttpConnectionDispatcherOptions(), configure);

        /// <summary>
        /// Maps incoming requests with the specified path to the provided connection pipeline.
        /// </summary>
        /// <typeparam name="TConnectionHandler">The <see cref="ConnectionHandler"/> type.</typeparam>
        /// <param name="endpoints">The <see cref="IEndpointBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <returns>An <see cref="ConnectionEndpointRouteBuilder"/> for endpoints associated with the connections.</returns>
        public static ConnectionEndpointRouteBuilder MapConnectionHandler<TConnectionHandler>(this IEndpointBuilder endpoints, string pattern) where TConnectionHandler : ConnectionHandler =>
            endpoints.MapConnectionHandler<TConnectionHandler>(pattern, configureOptions: null);

        /// <summary>
        /// Maps incoming requests with the specified path to the provided connection pipeline.
        /// </summary>
        /// <typeparam name="TConnectionHandler">The <see cref="ConnectionHandler"/> type.</typeparam>
        /// <param name="endpoints">The <see cref="IEndpointBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="configureOptions">A callback to configure dispatcher options.</param>
        /// <returns>An <see cref="ConnectionEndpointRouteBuilder"/> for endpoints associated with the connections.</returns>
        public static ConnectionEndpointRouteBuilder MapConnectionHandler<TConnectionHandler>(this IEndpointBuilder endpoints, string pattern, Action<HttpConnectionDispatcherOptions>? configureOptions) where TConnectionHandler : ConnectionHandler
        {
            var options = new HttpConnectionDispatcherOptions();
            configureOptions?.Invoke(options);

            var conventionBuilder = endpoints.MapConnections(pattern, options, b =>
            {
                b.UseConnectionHandler<TConnectionHandler>();
            });

            var attributes = typeof(TConnectionHandler).GetCustomAttributes(inherit: true);
            conventionBuilder.Add(e =>
            {
                // Add all attributes on the ConnectionHandler has metadata (this will allow for things like)
                // auth attributes and cors attributes to work seamlessly
                foreach (var item in attributes)
                {
                    e.Metadata.Add(item);
                }
            });

            return conventionBuilder;
        }

        /// <summary>
        /// Maps incoming requests with the specified path to the provided connection pipeline.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="options">Options used to configure the connection.</param>
        /// <param name="configure">A callback to configure the connection.</param>
        /// <returns>An <see cref="ConnectionEndpointRouteBuilder"/> for endpoints associated with the connections.</returns>
        public static ConnectionEndpointRouteBuilder MapConnections(this IEndpointBuilder endpoints, string pattern, HttpConnectionDispatcherOptions options, Action<IConnectionBuilder> configure)
        {
            var dispatcher = endpoints.ServiceProvider.GetRequiredService(_httpConnectionDispatcherType);

            var ExecuteNegotiateAsync = (Func<HttpContext, HttpConnectionDispatcherOptions, Task>)
                _httpConnectionDispatcherType
                    .GetMethod("ExecuteNegotiateAsync")!
                    .CreateDelegate(typeof(Func<HttpContext, HttpConnectionDispatcherOptions, Task>), dispatcher);

            var ExecuteAsync = (Func<HttpContext, HttpConnectionDispatcherOptions, ConnectionDelegate, Task>)
                _httpConnectionDispatcherType
                    .GetMethod("ExecuteAsync")!
                    .CreateDelegate(typeof(Func<HttpContext, HttpConnectionDispatcherOptions, ConnectionDelegate, Task>), dispatcher);

            var connectionBuilder = new ConnectionBuilder(endpoints.ServiceProvider);
            configure(connectionBuilder);
            var connectionDelegate = connectionBuilder.Build();

            // REVIEW: Consider expanding the internals of the dispatcher as endpoint routes instead of
            // using if statements we can let the matcher handle

            var conventionBuilders = new List<IEndpointConventionBuilder>();

            // Build the negotiate application
            var app = endpoints.CreateApplicationBuilder();
            app.UseWebSockets();
            app.Run(c => ExecuteNegotiateAsync(c, options));
            var negotiateHandler = app.Build();

            var negotiateBuilder = endpoints.MapRequestDelegate(pattern + "/negotiate", negotiateHandler);
            conventionBuilders.Add(negotiateBuilder);
            // Add the negotiate metadata so this endpoint can be identified
            negotiateBuilder.WithMetadata(new NegotiateMetadata());

            // build the execute handler part of the protocol
            app = endpoints.CreateApplicationBuilder();
            app.UseWebSockets();
            app.Run(c => ExecuteAsync(c, options, connectionDelegate));
            var executehandler = app.Build();

            var executeBuilder = endpoints.MapRequestDelegate(pattern, executehandler);
            conventionBuilders.Add(executeBuilder);

            var compositeConventionBuilder = new CompositeEndpointConventionBuilder(conventionBuilders);

            // Add metadata to all of Endpoints
            compositeConventionBuilder.Add(e =>
            {
                // Add the authorization data as metadata
                foreach (var data in options.AuthorizationData)
                {
                    e.Metadata.Add(data);
                }
            });

            return (ConnectionEndpointRouteBuilder)
                typeof(ConnectionEndpointRouteBuilder)
                    .GetTypeInfo().DeclaredConstructors
                    .Single()
                    .Invoke(new object[] { compositeConventionBuilder });
        }

        private class CompositeEndpointConventionBuilder : IEndpointConventionBuilder
        {
            private readonly List<IEndpointConventionBuilder> _endpointConventionBuilders;

            public CompositeEndpointConventionBuilder(List<IEndpointConventionBuilder> endpointConventionBuilders)
            {
                _endpointConventionBuilders = endpointConventionBuilders;
            }

            public void Add(Action<EndpointBuilder> convention)
            {
                foreach (var endpointConventionBuilder in _endpointConventionBuilders)
                {
                    endpointConventionBuilder.Add(convention);
                }
            }
        }
    }

    /// <summary>
    /// The extension methods for SignalR endpoint route builder.
    /// </summary>
    public static class SignalREndpointBuilderExtensions
    {
        private const string _signalRMarkerServiceTypeName = "Microsoft.Extensions.DependencyInjection.SignalRMarkerService";
        private static readonly Type _signalRMarkerServiceType
            = typeof(IHubEndpointConventionBuilder).Assembly.GetType(_signalRMarkerServiceTypeName)
            ?? throw new TypeLoadException("No " + _signalRMarkerServiceTypeName + " found.");

        /// <summary>
        /// Maps incoming requests with the specified path to the specified <see cref="Hub"/> type.
        /// </summary>
        /// <typeparam name="THub">The <see cref="Hub"/> type to map requests to.</typeparam>
        /// <param name="endpoints">The <see cref="IEndpointBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <returns>An <see cref="HubEndpointConventionBuilder"/> for endpoints associated with the connections.</returns>
        public static HubEndpointConventionBuilder MapHub<THub>(this IEndpointBuilder endpoints, string pattern) where THub : Hub =>
            endpoints.MapHub<THub>(pattern, configureOptions: null);

        /// <summary>
        /// Maps incoming requests with the specified path to the specified <see cref="Hub"/> type.
        /// </summary>
        /// <typeparam name="THub">The <see cref="Hub"/> type to map requests to.</typeparam>
        /// <param name="endpoints">The <see cref="IEndpointBuilder"/> to add the route to.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="configureOptions">A callback to configure dispatcher options.</param>
        /// <returns>An <see cref="HubEndpointConventionBuilder"/> for endpoints associated with the connections.</returns>
        public static HubEndpointConventionBuilder MapHub<THub>(this IEndpointBuilder endpoints, string pattern, Action<HttpConnectionDispatcherOptions>? configureOptions) where THub : Hub
        {
            var marker = endpoints.ServiceProvider.GetService(_signalRMarkerServiceType);

            if (marker == null)
            {
                throw new InvalidOperationException(
                    "Unable to find the required services. Please add all the required services by calling " +
                    "'IServiceCollection.AddSignalR' inside the call to 'ConfigureServices(...)' in the application startup code.");
            }

            var options = new HttpConnectionDispatcherOptions();
            configureOptions?.Invoke(options);

            var conventionBuilder = endpoints.MapConnections(pattern, options, b =>
            {
                b.UseHub<THub>();
            });

            var attributes = typeof(THub).GetCustomAttributes(inherit: true);
            conventionBuilder.Add(e =>
            {
                // Add all attributes on the Hub as metadata (this will allow for things like)
                // auth attributes and cors attributes to work seamlessly
                foreach (var item in attributes)
                {
                    e.Metadata.Add(item);
                }

                // Add metadata that captures the hub type this endpoint is associated with
                e.Metadata.Add(new HubMetadata(typeof(THub)));
            });

            return (HubEndpointConventionBuilder)
                typeof(HubEndpointConventionBuilder)
                    .GetTypeInfo().DeclaredConstructors
                    .Single()
                    .Invoke(new object[] { conventionBuilder });
        }
    }

    /// <summary>
    /// The extension methods for Blazor endpoint route builder.
    /// </summary>
    public static class BlazorEndpointBuilderExtensions
    {
        const string ComponentHubDefaultPath = "/_blazor";

        private const string _componentHubTypeName = "Microsoft.AspNetCore.Components.Server.ComponentHub";
        private static readonly Type _componentHubType
            = typeof(CircuitOptions).Assembly.GetType(_componentHubTypeName)
            ?? throw new TypeLoadException("No " + _componentHubTypeName + " found.");

        private const string _circuitDisconnectMiddlewareTypeName = "Microsoft.AspNetCore.Components.Server.CircuitDisconnectMiddleware";
        private static readonly Type _circuitDisconnectMiddlewareType
            = typeof(CircuitOptions).Assembly.GetType(_circuitDisconnectMiddlewareTypeName)
            ?? throw new TypeLoadException("No " + _circuitDisconnectMiddlewareTypeName + " found.");

        private const string _circuitJavaScriptInitializationMiddlewareTypeName = "Microsoft.AspNetCore.Builder.CircuitJavaScriptInitializationMiddleware";
        private static readonly Type _circuitJavaScriptInitializationMiddlewareType
            = typeof(CircuitOptions).Assembly.GetType(_circuitJavaScriptInitializationMiddlewareTypeName)
            ?? throw new TypeLoadException("No " + _circuitJavaScriptInitializationMiddlewareTypeName + " found.");

        private static readonly Func<IEndpointBuilder, string, Action<HttpConnectionDispatcherOptions>, HubEndpointConventionBuilder> _mapComponentHubDelegate
            = (Func<IEndpointBuilder, string, Action<HttpConnectionDispatcherOptions>, HubEndpointConventionBuilder>)
                new Func<IEndpointBuilder, string, Action<HttpConnectionDispatcherOptions>, HubEndpointConventionBuilder>(
                    SignalREndpointBuilderExtensions.MapHub<Hub>)
                    .GetMethodInfo()!
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(_componentHubType)
                    .CreateDelegate(typeof(Func<IEndpointBuilder, string, Action<HttpConnectionDispatcherOptions>, HubEndpointConventionBuilder>));

        /// <summary>
        /// Maps the Blazor <see cref="Hub" /> to the default path.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointBuilder"/>.</param>
        /// <returns>The <see cref="ComponentEndpointConventionBuilder"/>.</returns>
        public static ComponentEndpointConventionBuilder MapBlazorHub(this IEndpointBuilder endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            return endpoints.MapBlazorHub(ComponentHubDefaultPath);
        }

        /// <summary>
        /// Maps the Blazor <see cref="Hub" /> to the path <paramref name="path"/>.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointBuilder"/>.</param>
        /// <param name="path">The path to map the Blazor <see cref="Hub" />.</param>
        /// <returns>The <see cref="ComponentEndpointConventionBuilder"/>.</returns>
        public static ComponentEndpointConventionBuilder MapBlazorHub(this IEndpointBuilder endpoints, string path)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return endpoints.MapBlazorHub(path, configureOptions: _ => { });
        }

        /// <summary>
        /// Maps the Blazor <see cref="Hub" /> to the default path.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointBuilder"/>.</param>
        /// <param name="configureOptions">A callback to configure dispatcher options.</param>
        /// <returns>The <see cref="ComponentEndpointConventionBuilder"/>.</returns>
        public static ComponentEndpointConventionBuilder MapBlazorHub(this IEndpointBuilder endpoints, Action<HttpConnectionDispatcherOptions> configureOptions)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            return endpoints.MapBlazorHub(ComponentHubDefaultPath, configureOptions);
        }

        /// <summary>
        /// Maps the Blazor <see cref="Hub" /> to the path <paramref name="path"/>.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointBuilder"/>.</param>
        /// <param name="path">The path to map the Blazor <see cref="Hub" />.</param>
        /// <param name="configureOptions">A callback to configure dispatcher options.</param>
        /// <returns>The <see cref="ComponentEndpointConventionBuilder"/>.</returns>
        public static ComponentEndpointConventionBuilder MapBlazorHub(this IEndpointBuilder endpoints, string path, Action<HttpConnectionDispatcherOptions> configureOptions)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            var hubEndpoint = _mapComponentHubDelegate.Invoke(endpoints, path, configureOptions);

            var disconnectEndpoint = endpoints
                .MapRequestDelegate(
                    (path.EndsWith('/') ? path : path + "/") + "disconnect/",
                    endpoints.CreateApplicationBuilder().UseMiddleware(_circuitDisconnectMiddlewareType).Build())
                .WithDisplayName("Blazor disconnect");

            var jsInitializersEndpoint = endpoints
                .MapRequestDelegate(
                    (path.EndsWith('/') ? path : path + "/") + "initializers/",
                    endpoints.CreateApplicationBuilder().UseMiddleware(_circuitJavaScriptInitializationMiddlewareType).Build())
                .WithDisplayName("Blazor initializers");

            return (ComponentEndpointConventionBuilder)
                typeof(ComponentEndpointConventionBuilder)
                    .GetTypeInfo().DeclaredConstructors
                    .Single()
                    .Invoke(new object[] { hubEndpoint, disconnectEndpoint, jsInitializersEndpoint });
        }
    }

    /// <summary>
    /// The extension methods for substrate endpoint route builder.
    /// </summary>
    public static class SubstrateEndpointBuilderExtensions
    {
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
        /// <returns>The route builder</returns>
        internal static void MapModules(this IEndpointRouteBuilder builder)
        {
            var menu = builder.ServiceProvider.GetRequiredService<ConcreteMenuContributor>();
            var modules = builder.ServiceProvider.GetRequiredService<ReadOnlyCollection<AbstractModule>>();
            var connectors = builder.ServiceProvider.GetRequiredService<ReadOnlyCollection<AbstractConnector>>();
            menu.Store.Add(MenuNameDefaults.UserDropdown, new ConcreteSubmenuBuilder(menu) { Metadata = { ["Link"] = "", ["Icon"] = "", ["Title"] = "" } });

            foreach (var module in modules)
            {
                module.RegisterEndpoints(ModuleEndpointDataSource.CreateBuilder(module, builder));
                module.RegisterMenu(menu);
            }

            foreach (var connector in connectors)
            {
                connector.RegisterEndpoints(new ConnectorEndpointBuilder(builder, connector.Area, connector));
                connector.RegisterMenu(menu);
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
