using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Routing
{
    internal class DefaultErrorHandlerBuilder : IErrorHandlerBuilder
    {
        public ControllerActionDescriptorWrapper ActionDescriptor { get; }

        public IEndpointBuilder Builder { get; }

        public DefaultErrorHandlerBuilder(ControllerActionDescriptorWrapper actionDescriptor, IEndpointBuilder builder)
        {
            ActionDescriptor = actionDescriptor;
            Builder = builder;
        }

        public IErrorHandlerBuilder MapFallbackNotFound(string pattern)
        {
            var actionLazy = ActionDescriptor;

            Builder.MapFallback(actionLazy.GetPattern(pattern), context =>
            {
                context.Response.StatusCode = 404;

                var routeData = new RouteData();
                routeData.PushState(router: null, context.Request.RouteValues, new RouteValueDictionary());
                var actionContext = new ActionContext(context, routeData, actionLazy.GetValue(context.RequestServices));

                var invoker = context.RequestServices
                    .GetRequiredService<IActionInvokerFactory>()
                    .CreateInvoker(actionContext);

                return invoker.InvokeAsync();
            })
            .WithDisplayName($"Status Code Page {pattern} (Fallback NotFound)");

            return this;
        }

        public IErrorHandlerBuilder MapStatusCode(string pattern)
        {
            Builder.ServiceProvider
                .GetRequiredService<ReExecuteEndpointDataSource>()
                .Add(pattern, ActionDescriptor);
            return this;
        }
    }
}
