using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNetCore.Routing
{
    internal abstract class ModuleEndpointDataSource : EndpointDataSource
    {
        private static readonly object _locker = new object();
        private static readonly Func<object, List<Action<EndpointBuilder>>, ControllerActionEndpointConventionBuilder> _cbFactory;
        private static readonly Type _actionEndpointFactoryType;
        private static readonly Action<object, List<Endpoint>, HashSet<string>, ActionDescriptor, IReadOnlyList<Action<EndpointBuilder>>, bool> AddEndpoints;
        private static readonly MethodInfo _createCoreMethod;

        private List<Endpoint>? _endpoints;
        private readonly object _actionEndpointFactory;
        private readonly IActionDescriptorCollectionProvider _actions;
        private readonly Assembly _moduleAssembly;
        private readonly Type _moduleType;
        private readonly Type _moduleAbstractType;
        private readonly List<DefaultEndpointConventionBuilder> _modelEndpoints;

        static ModuleEndpointDataSource()
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

            _createCoreMethod = new Func<AbstractModule, IEndpointRouteBuilder, DefaultEndpointBuilder<AbstractModule>>(CreateCore)
                .GetMethodInfo()!
                .GetGenericMethodDefinition();
        }

        public ModuleEndpointDataSource(IServiceProvider serviceProvider, Type parentType)
        {
            ControllerRouteConventions = new List<Action<EndpointBuilder>>();
            ControllerRouteConventionBuilder = _cbFactory(_locker, ControllerRouteConventions);
            _actionEndpointFactory = serviceProvider.GetRequiredService(_actionEndpointFactoryType)!;
            _actions = serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();
            _moduleAssembly = parentType.Assembly;
            _moduleType = parentType;
            _moduleAbstractType = parentType.IsConstructedGenericType ? parentType.GetGenericTypeDefinition() : parentType;
            _modelEndpoints = new List<DefaultEndpointConventionBuilder>();
        }

        public List<Action<EndpointBuilder>> ControllerRouteConventions { get; }

        public ControllerActionEndpointConventionBuilder ControllerRouteConventionBuilder { get; }

        public bool EnableController { get; set; }

        public IEndpointConventionBuilder AddRequestDelegate(RoutePattern routePattern, RequestDelegate requestDelegate)
        {
            var builder = new DefaultEndpointConventionBuilder(routePattern, requestDelegate);
            _modelEndpoints.Add(builder);
            return builder;
        }

        public static IEndpointBuilder CreateBuilder(AbstractModule module, IEndpointRouteBuilder builder)
        {
            return (IEndpointBuilder)
                _createCoreMethod.MakeGenericMethod(module.GetType())
                .Invoke(null, new object[] { module, builder })!;
        }

        private static DefaultEndpointBuilder<TModule> CreateCore<TModule>(TModule module, IEndpointRouteBuilder builder) where TModule : AbstractModule
        {
            return new DefaultEndpointBuilder<TModule>(builder, module.Area, module.Conventions);
        }

        private bool CheckEligibility(ActionDescriptor descriptor, out ControllerActionDescriptor? action)
        {
            action = descriptor as ControllerActionDescriptor;
            if (action == null) return false;
            if (action.ControllerTypeInfo.Assembly == _moduleAssembly) return true;

            // when a controller is affiliated to this module
            var aff = action.ControllerTypeInfo.Assembly.GetCustomAttribute<AffiliateToAttribute>()?.BelongingModuleType;
            if (aff == _moduleAbstractType || aff == _moduleType) return true;

            // not belong to this module, bye
            return false;
        }

        private void Initialize()
        {
            List<Endpoint> CreateEndpoints()
            {
                var endpoints = new List<Endpoint>();

                if (EnableController)
                {
                    var routeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var actions = _actions.ActionDescriptors.Items;
                    var conventions = ControllerRouteConventions;

                    for (var i = 0; i < actions.Count; i++)
                    {
                        if (CheckEligibility(actions[i], out var action) && action != null)
                        {
                            AddEndpoints(_actionEndpointFactory, endpoints, routeNames, action, conventions, false);
                        }
                    }
                }

                foreach (var item in _modelEndpoints)
                {
                    endpoints.Add(item.Build());
                }

                return endpoints;
            }

            if (_endpoints == null)
            {
                lock (_locker)
                {
                    if (_endpoints == null)
                    {
                        _endpoints = CreateEndpoints();
                    }
                }
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

    internal class ModuleEndpointDataSource<TModule> : ModuleEndpointDataSource where TModule : AbstractModule
    {
        public ModuleEndpointDataSource(IServiceProvider serviceProvider) : base(serviceProvider, typeof(TModule))
        {
        }
    }
}
