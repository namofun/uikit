﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNetCore.Routing
{
    /// <inheritdoc />
    public sealed class OrderLinkGenerator : LinkGenerator, IDisposable
    {
        private readonly LinkGenerator inner;
        private readonly TemplateBinderFactory _binderFactory;
        private readonly Func<RouteEndpoint, TemplateBinder> _createTemplateBinder;
        private readonly FieldInfo _requiredKeys;
        const string typeName = "Microsoft.AspNetCore.Routing.DefaultLinkGenerator";
        internal static readonly Type? typeInner = typeof(Route).Assembly.GetType("Microsoft.AspNetCore.Routing.DefaultLinkGenerator");

        /// <summary>
        /// Create instance of <see cref="LinkGenerator"/> that wrapped the real generator inner.
        /// </summary>
        /// <param name="parameterPolicyFactory">The parameter policy factory</param>
        /// <param name="binderFactory">The template binder factory</param>
        /// <param name="dataSource">The endpoint data source</param>
        /// <param name="routeOptions">The route options</param>
        /// <param name="serviceProvider">The service provider</param>
        public OrderLinkGenerator(
            ParameterPolicyFactory parameterPolicyFactory,
            TemplateBinderFactory binderFactory,
            EndpointDataSource dataSource,
            IOptions<RouteOptions> routeOptions,
            IServiceProvider serviceProvider)
        {
            if (typeInner!.FullName != typeName)
                throw new NotImplementedException();
            var logger = serviceProvider.GetService(typeof(ILogger<>).MakeGenericType(typeInner));
            var autoFlag = BindingFlags.NonPublic | BindingFlags.Instance;

            var args = new object[]
            {
                parameterPolicyFactory,
                binderFactory,
                dataSource,
                routeOptions,
                logger,
                serviceProvider
            };

            var ctorInfo = typeInner.GetConstructors()[0];
            var newExp = Expression.New(ctorInfo, args.Select(o => Expression.Constant(o)));
            var ctor = Expression.Lambda<Func<LinkGenerator>>(newExp).Compile();
            inner = ctor();

            _binderFactory = binderFactory;
            _createTemplateBinder = CreateTemplateBinder;
            var fieldInfo = typeInner.GetField(nameof(_createTemplateBinder), autoFlag);
            fieldInfo!.SetValue(inner, _createTemplateBinder);

            _requiredKeys = typeof(TemplateBinder).GetField(nameof(_requiredKeys), autoFlag)!;
        }

        /// <summary>
        /// Create <see cref="TemplateBinder"/> instance by <paramref name="endpoint"/>.
        /// </summary>
        /// <param name="endpoint">The <see cref="RouteEndpoint"/></param>
        /// <returns>The <see cref="TemplateBinder"/></returns>
        private TemplateBinder CreateTemplateBinder(RouteEndpoint endpoint)
        {
            /*
             * The following code section is disabled
             * for its change to RoutePattern may cause
             * errors.
             * 
             * var rawText = endpoint.RoutePattern.RawText;
             * var rv = endpoint.RoutePattern.RequiredValues as RouteValueDictionary;
             *
             * if (rawText != null)
             * {
             *     var m = Regex.Matches(rawText, "\\{(\\w+)\\}");
             *     for (int i = 0; i < m.Count; i++)
             *         rv.Add(m[i].Value.TrimStart('{').TrimEnd('}'), RoutePattern.RequiredValueAny);
             * }
             * 
             * A better solution is to disable the _requiredKeys.
             */

            var binder = _binderFactory.Create(endpoint.RoutePattern);
            _requiredKeys.SetValue(binder, Array.Empty<string>());
            return binder;
        }

        /// <inheritdoc />
        public override string GetPathByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values, RouteValueDictionary? ambientValues = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions? options = null) =>
            inner.GetPathByAddress(httpContext, address, values, ambientValues, pathBase, fragment, options);

        /// <inheritdoc />
        public override string GetPathByAddress<TAddress>(TAddress address, RouteValueDictionary values, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null) =>
            inner.GetPathByAddress(address, values, pathBase, fragment, options);

        /// <inheritdoc />
        public override string GetUriByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values, RouteValueDictionary? ambientValues = null, string? scheme = null, HostString? host = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions? options = null) =>
            inner.GetUriByAddress(httpContext, address, values, ambientValues, scheme, host, pathBase, fragment, options);

        /// <inheritdoc />
        public override string GetUriByAddress<TAddress>(TAddress address, RouteValueDictionary values, string scheme, HostString host, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null) =>
            inner.GetUriByAddress(address, values, scheme, host, pathBase, fragment, options);

        /// <inheritdoc />
        public void Dispose() => ((IDisposable)inner).Dispose();
    }
}