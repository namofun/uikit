using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    internal abstract class ModuleEndpointDataSourceBase : EndpointDataSource
    {
        private static readonly object _locker = new object();
        protected static readonly Func<object, List<Action<EndpointBuilder>>, ControllerActionEndpointConventionBuilder> _cbFactory;
        private static readonly Type _actionEndpointFactoryType;
        private static readonly Action<object, List<Endpoint>, HashSet<string>, ActionDescriptor, IReadOnlyList<Action<EndpointBuilder>>, bool> AddEndpoints;
        private List<Endpoint>? _endpoints;
        private readonly object _actionEndpointFactory;
        private readonly IActionDescriptorCollectionProvider _actions;
        private readonly Assembly _moduleAssembly;

        static ModuleEndpointDataSourceBase()
        {
            var ctor = typeof(ControllerActionEndpointConventionBuilder)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single();
            var para_1 = Expression.Parameter(typeof(object), "locker");
            var para_2 = Expression.Parameter(typeof(List<Action<EndpointBuilder>>), "laeb");
            var body = Expression.New(ctor, para_1, para_2);
            var func = Expression.Lambda<Func<object, List<Action<EndpointBuilder>>, ControllerActionEndpointConventionBuilder>>(body, para_1, para_2);
            _cbFactory = func.Compile();

            _actionEndpointFactoryType = Assembly.GetAssembly(typeof(ApplicationModel))!
                .GetType("Microsoft.AspNetCore.Mvc.Routing.ActionEndpointFactory")
                ?? throw new InvalidOperationException();
            var method = _actionEndpointFactoryType.GetMethod("AddEndpoints")!;
            var para1 = Expression.Parameter(typeof(object), "factory");
            var para1r = Expression.Convert(para1, _actionEndpointFactoryType);
            var para2 = Expression.Parameter(typeof(List<Endpoint>), "endpoints");
            var para3 = Expression.Parameter(typeof(HashSet<string>), "routeNames");
            var para4 = Expression.Parameter(typeof(ActionDescriptor), "action");
            var _conventionalRouteEntryType = method.GetParameters()[3].ParameterType.GenericTypeArguments[0];
            var const5 = Expression.New(typeof(List<>).MakeGenericType(_conventionalRouteEntryType));
            var value5 = Expression.Lambda<Func<object>>(const5).Compile().Invoke();
            var para5 = Expression.Constant(value5, typeof(List<>).MakeGenericType(_conventionalRouteEntryType)); // routes
            var para6 = Expression.Parameter(typeof(IReadOnlyList<Action<EndpointBuilder>>), "conventions");
            var para7 = Expression.Parameter(typeof(bool), "createInertEndpoints");
            var body2 = Expression.Call(para1r, method, para2, para3, para4, para5, para6, para7);
            var func2 = Expression.Lambda<Action<object, List<Endpoint>, HashSet<string>, ActionDescriptor, IReadOnlyList<Action<EndpointBuilder>>, bool>>(body2, para1, para2, para3, para4, para6, para7);
            AddEndpoints = func2.Compile();
        }

        public ModuleEndpointDataSourceBase(IServiceProvider serviceProvider, Type parentType)
        {
            Conventions = new List<Action<EndpointBuilder>>();
            ConventionBuilder = _cbFactory(_locker, Conventions);
            _actionEndpointFactory = serviceProvider.GetRequiredService(_actionEndpointFactoryType)!;
            _actions = serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();
            _moduleAssembly = parentType.Assembly;
        }

        public List<Action<EndpointBuilder>> Conventions { get; }

        public ControllerActionEndpointConventionBuilder ConventionBuilder { get; }

        public static IEndpointBuilder Factory(AbstractModule module, IEndpointRouteBuilder builder)
        {
            return (IEndpointBuilder)typeof(ModuleEndpointDataSourceBase)
                .GetMethod(nameof(FactoryInternal))!
                .MakeGenericMethod(module.GetType())
                .Invoke(null, new object[] { module, builder })!;
        }

        public static DefaultEndpointBuilder<TModule> FactoryInternal<TModule>(TModule module, IEndpointRouteBuilder builder) where TModule : AbstractModule
        {
            return new DefaultEndpointBuilder<TModule>(builder, module.Area, module.Conventions);
        }

        private void Initialize()
        {
            if (_endpoints == null)
            lock (_locker)
            if (_endpoints == null)
            {
                var endpoints = new List<Endpoint>();
                var routeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var actions = _actions.ActionDescriptors.Items;
                var conventions = Conventions;

                for (var i = 0; i < actions.Count; i++)
                {
                    if (actions[i] is ControllerActionDescriptor action && action.ControllerTypeInfo.Assembly == _moduleAssembly)
                    {
                        AddEndpoints(_actionEndpointFactory, endpoints, routeNames, action, conventions, false);
                    }
                }

                _endpoints = endpoints;
            }
        }

        public override IReadOnlyList<Endpoint> Endpoints
        {
            get
            {
                Initialize();
                Debug.Assert(_endpoints != null);
                return _endpoints;
            }
        }

        public override IChangeToken GetChangeToken()
        {
            return NullChangeToken.Singleton;
        }
    }

    internal class ModuleEndpointDataSource<TModule> : ModuleEndpointDataSourceBase where TModule : AbstractModule
    {
        public ModuleEndpointDataSource(IServiceProvider serviceProvider) : base(serviceProvider, typeof(TModule))
        {
        }
    }

    internal static class EndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Adds a specialized <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/>
        /// that will match the provided pattern with the lowest possible priority.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public static IEndpointConventionBuilder MapNotFound(this IEndpointRouteBuilder endpoints, string pattern)
        {
            return endpoints.MapFallback(pattern, context =>
            {
                context.Features.Set<IClaimedNoStatusCodePageFeature>(new ClaimedNoStatusCodePageFeature());
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
            });
        }
    }

    public class ControllerActionDescriptorLazy
    {
        private ControllerActionDescriptor? _value;
        private readonly string _area, _controller, _action;

        public ControllerActionDescriptorLazy(string area, string controller, string action)
        {
            _area = area;
            _controller = controller;
            _action = action;
        }

        public ControllerActionDescriptor GetValue(IServiceProvider services)
        {
            if (_value != null) return _value;

            var adcp = services.GetRequiredService<IActionDescriptorCollectionProvider>();
            var actions = adcp.ActionDescriptors.Items;
            var action = actions.OfType<ControllerActionDescriptor>()
                .Where(s => s.ControllerName.Equals(_controller, StringComparison.OrdinalIgnoreCase))
                .Where(s => s.ActionName.Equals(_action, StringComparison.OrdinalIgnoreCase))
                .Where(s => s.RouteValues.TryGetValue("area", out var AreaName) && AreaName.Equals(_area, StringComparison.OrdinalIgnoreCase))
                .Single();

            return _value = action;
        }
    }

    internal class DefaultEndpointBuilder<TModule> : IEndpointBuilder where TModule : AbstractModule
    {
        public IEndpointRouteBuilder Builder { get; }

        public Action<IEndpointConventionBuilder> DefaultConvention { get; }

        public string AreaName { get; }

        public DefaultEndpointBuilder(IEndpointRouteBuilder builder, string areaName, Action<IEndpointConventionBuilder> convention)
        {
            AreaName = areaName;
            Builder = builder;
            DefaultConvention = convention;
        }

        private T GetRequiredService<T>()
        {
            return Builder.ServiceProvider.GetRequiredService<T>();
        }

        private ModuleEndpointDataSource<TModule> GetOrCreateDataSource()
        {
            var dataSource = Builder.DataSources.OfType<ModuleEndpointDataSource<TModule>>().FirstOrDefault();
            if (dataSource == null)
            {
                dataSource = GetRequiredService<ModuleEndpointDataSource<TModule>>();
                Builder.DataSources.Add(dataSource);
            }

            return dataSource;
        }

        public IEndpointConventionBuilder MapApiDocument(string name, string title, string description, string version)
        {
            var assembly = typeof(TModule).Assembly;
            GetRequiredService<SubstrateControllerConvention>()
                .Declare(assembly.FullName!, name);

            var sgo = GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

            sgo.SwaggerDoc(name, new OpenApiInfo
            {
                Title = title,
                Description = description,
                Version = version,
            });

            var file = System.IO.Path.ChangeExtension(assembly.Location, "xml");
            if (System.IO.File.Exists(file))
            {
                sgo.IncludeXmlComments(file);
            }
            else
            {
                GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Microsoft.Hosting.Lifetime")
                    .LogWarning($"Documentation '{file}' is not found. Specification comments will not be registered into swagger.");
            }

            var actionLazy = new ControllerActionDescriptorLazy("Dashboard", "ApiDoc", "Display");
            return Builder.MapGet("/api/doc/" + name, context =>
            {
                var routeData = new RouteData();
                routeData.PushState(router: null, context.Request.RouteValues, new RouteValueDictionary());
                routeData.Values["name"] = name;
                var actionContext = new ActionContext(context, routeData, actionLazy.GetValue(context.RequestServices));

                var invoker = context.RequestServices
                    .GetRequiredService<IActionInvokerFactory>()
                    .CreateInvoker(actionContext);

                return invoker.InvokeAsync();
            })
            .WithDisplayName(_ => $"Swagger Document ({name})")
            .WithDefaults(DefaultConvention);
        }

        public ControllerActionEndpointConventionBuilder MapControllers()
        {
            return GetOrCreateDataSource().ConventionBuilder.WithDefaults(DefaultConvention);
        }

        public IErrorHandlerBuilder WithErrorHandler(string area, string controller, string action)
        {
            var ad = new ControllerActionDescriptorLazy(area, controller, action);
            return new DefaultErrorHandlerBuilder(ad, Builder, DefaultConvention);
        }

        public IEndpointConventionBuilder MapFallNotFound(string pattern)
        {
            return Builder.MapNotFound(pattern).WithDefaults(DefaultConvention);
        }

        public IEndpointConventionBuilder MapFallback(string pattern, RequestDelegate requestDelegate)
        {
            return Builder.MapFallback(pattern, requestDelegate).WithDefaults(DefaultConvention);
        }

        public HubEndpointConventionBuilder MapHub<THub>(string pattern, Action<HttpConnectionDispatcherOptions>? configureOptions = null) where THub : Hub
        {
            return Builder.MapHub<THub>(pattern, configureOptions ?? (options => { })).WithDefaults(DefaultConvention);
        }

        public IEndpointConventionBuilder MapRequestDelegate(string pattern, RequestDelegate requestDelegate)
        {
            return Builder.Map(pattern, requestDelegate).WithDefaults(DefaultConvention);
        }
    }

    internal class DefaultErrorHandlerBuilder : IErrorHandlerBuilder
    {
        public ControllerActionDescriptorLazy ActionDescriptor { get; }

        public Action<IEndpointConventionBuilder> DefaultConvention { get; }

        public IEndpointRouteBuilder Builder { get; }

        public DefaultErrorHandlerBuilder(ControllerActionDescriptorLazy actionDescriptor, IEndpointRouteBuilder builder, Action<IEndpointConventionBuilder> convention)
        {
            ActionDescriptor = actionDescriptor;
            Builder = builder;
            DefaultConvention = convention;
        }

        public IErrorHandlerBuilder MapFallbackNotFound(string pattern)
        {
            var action = ActionDescriptor;

            Builder.MapFallback(pattern, context =>
            {
                context.Response.StatusCode = 404;

                var routeData = new RouteData();
                routeData.PushState(router: null, context.Request.RouteValues, new RouteValueDictionary());
                var actionContext = new ActionContext(context, routeData, action.GetValue(context.RequestServices));

                var invoker = context.RequestServices
                    .GetRequiredService<IActionInvokerFactory>()
                    .CreateInvoker(actionContext);

                return invoker.InvokeAsync();
            })
            .WithDefaults(DefaultConvention);

            return this;
        }

        public IErrorHandlerBuilder MapStatusCode(string pattern)
        {
            Builder.ServiceProvider
                .GetRequiredService<ReExecuteEndpointMatcher>()
                .Add(pattern, ActionDescriptor);
            return this;
        }
    }
}
