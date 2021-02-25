using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    /// <summary>
    /// A substrate implementation of <see cref="IUrlHelperFactory"/>.
    /// </summary>
    public class SubstrateUrlHelperFactory : IUrlHelperFactory
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IReadOnlyList<IRewriteRule> _rewriteRules;
        private const string SuppressFlag = "__SuppressOutboundRewriting";

        // Lambdas hoisted to static readonly fields to improve inlining https://github.com/dotnet/roslyn/issues/13624
        private readonly static Func<ActionContext, LinkGenerator, IReadOnlyList<IRewriteRule>, IUrlHelper> _nRewriteUrlHelper = (context, linkGenerator, rewriteRules) => new RewriteUrlHelper(context, linkGenerator, rewriteRules);
        private readonly static Func<ActionContext, LinkGenerator, IUrlHelper> _nEndpointRoutingUrlHelper;

        static SubstrateUrlHelperFactory()
        {
            var contextParam = Expression.Parameter(typeof(ActionContext), "context");
            var linkGeneratorParam = Expression.Parameter(typeof(LinkGenerator), "linkGenerator");
            var httpContextParam = Expression.Property(contextParam, nameof(ActionContext.HttpContext));
            var requestServiceParam = Expression.Property(httpContextParam, nameof(Http.HttpContext.RequestServices));
            var helperType = typeof(UrlHelperBase).Assembly.GetType("Microsoft.AspNetCore.Mvc.Routing.EndpointRoutingUrlHelper")!;
            var grsMethod = new Func<IServiceProvider, object>(ServiceProviderServiceExtensions.GetRequiredService<object>).GetMethodInfo()!.GetGenericMethodDefinition();
            var body = Expression.New(
                helperType.GetConstructors().Single(),
                contextParam,
                linkGeneratorParam,
                Expression.Call(grsMethod.MakeGenericMethod(typeof(ILogger<>).MakeGenericType(helperType)), requestServiceParam));
            _nEndpointRoutingUrlHelper = Expression.Lambda<Func<ActionContext, LinkGenerator, IUrlHelper>>(body, contextParam, linkGeneratorParam).Compile();
        }

        /// <summary>
        /// Initialize the <see cref="SubstrateUrlHelperFactory"/>.
        /// </summary>
        /// <param name="linkGenerator">The link generator.</param>
        /// <param name="rewriteRules">The rewrite rules.</param>
        public SubstrateUrlHelperFactory(LinkGenerator linkGenerator, IEnumerable<IRewriteRule> rewriteRules)
        {
            _linkGenerator = linkGenerator;

            if (rewriteRules.Any())
            {
                var list = rewriteRules.Prepend(new PreparationRule()).ToList();
                _rewriteRules = new ReadOnlyCollection<IRewriteRule>(list);
            }
            else
            {
                _rewriteRules = Array.Empty<IRewriteRule>();
            }
        }

        /// <summary>
        /// The rewrite rules
        /// </summary>
        public IReadOnlyList<IRewriteRule> RewriteRules => _rewriteRules;

        /// <summary>
        /// Whether to enable url rewriting
        /// </summary>
        public bool Enabled { get; internal set; }

        /// <inheritdoc />
        public IUrlHelper GetUrlHelper(ActionContext context)
        {
            if (context?.HttpContext?.Items == null)
                throw new ArgumentNullException(nameof(context));
            var httpContext = context.HttpContext;

            // Perf: Create only one UrlHelper per context
            if (httpContext.Items.TryGetValue(typeof(IUrlHelper), out var value) && value is IUrlHelper urlHelper)
                return urlHelper;

            if (_rewriteRules.Count == 0 || !Enabled || httpContext.Items.ContainsKey(SuppressFlag))
                urlHelper = _nEndpointRoutingUrlHelper(context, _linkGenerator);
            else
                urlHelper = _nRewriteUrlHelper(context, _linkGenerator, _rewriteRules);

            httpContext.Items[typeof(IUrlHelper)] = urlHelper;
            return urlHelper;
        }

        /// <summary>
        /// Copied from <see cref="EndpointRoutingUrlHelper"/>.
        /// </summary>
        internal class RewriteUrlHelper : UrlHelperBase
        {
            private readonly LinkGenerator _linkGenerator;
            private readonly IReadOnlyList<IRewriteRule> _rewriteRules;

            public RewriteUrlHelper(
                ActionContext actionContext,
                LinkGenerator linkGenerator,
                IReadOnlyList<IRewriteRule> rewriteRules)
                : base(actionContext)
            {
                _linkGenerator = linkGenerator;
                _rewriteRules = rewriteRules;
            }

            public override string Action(UrlActionContext urlActionContext)
            {
                if (urlActionContext == null)
                {
                    throw new ArgumentNullException(nameof(urlActionContext));
                }

                var values = GetValuesDictionary(urlActionContext.Values);

                if (urlActionContext.Action == null)
                {
                    if (!values.ContainsKey("action") &&
                        AmbientValues.TryGetValue("action", out var action))
                    {
                        values["action"] = action;
                    }
                }
                else
                {
                    values["action"] = urlActionContext.Action;
                }

                if (urlActionContext.Controller == null)
                {
                    if (!values.ContainsKey("controller") &&
                        AmbientValues.TryGetValue("controller", out var controller))
                    {
                        values["controller"] = controller;
                    }
                }
                else
                {
                    values["controller"] = urlActionContext.Controller;
                }


                var path = _linkGenerator.GetPathByRouteValues(
                    ActionContext.HttpContext,
                    routeName: null,
                    values,
                    fragment: urlActionContext.Fragment == null ? FragmentString.Empty : new FragmentString("#" + urlActionContext.Fragment));
                return GenerateUrl2(urlActionContext.Protocol, urlActionContext.Host, path);
            }

            public override string RouteUrl(UrlRouteContext routeContext)
            {
                if (routeContext == null)
                {
                    throw new ArgumentNullException(nameof(routeContext));
                }

                var path = _linkGenerator.GetPathByRouteValues(
                    ActionContext.HttpContext,
                    routeContext.RouteName,
                    routeContext.Values,
                    fragment: routeContext.Fragment == null ? FragmentString.Empty : new FragmentString("#" + routeContext.Fragment));
                return GenerateUrl2(routeContext.Protocol, routeContext.Host, path);
            }

            private string GenerateUrl2(string? protocol, string? host, string? path)
            {
                for (int i = 0; path != null && i < _rewriteRules.Count; i++)
                {
                    if (_rewriteRules[i].ApplyUrl(ActionContext, ref path) != Rewrite.RuleResult.ContinueRules)
                    {
                        break;
                    }
                }

                return GenerateUrl(protocol, host, path);
            }
        }
    }
}
