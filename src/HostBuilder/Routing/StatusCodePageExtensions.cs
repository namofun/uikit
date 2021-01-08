using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Methods using claims to create customized status code page.
    /// </summary>
    public static class StatusCodePageExtensions
    {
        private class CatchExceptionMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<CatchExceptionMiddleware> _logger;
            private readonly DiagnosticSource _diagnosticSource;
            private const string _une = "Microsoft.AspNetCore.Diagnostics.UnhandledException";

            public CatchExceptionMiddleware(RequestDelegate next,
                ILogger<CatchExceptionMiddleware> logger,
                DiagnosticSource diagnosticSource)
            {
                _next = next;
                _logger = logger;
                _diagnosticSource = diagnosticSource;
            }

            public async Task Invoke(HttpContext context)
            {
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled exception has occurred while executing the request.");

                    if (context.Response.HasStarted)
                    {
                        _logger.LogWarning("Response has been started, rethrowing...");
                        throw;
                    }

                    try
                    {
                        context.Response.Clear();
                        context.Response.StatusCode = 500;
                        if (_diagnosticSource.IsEnabled(_une))
                            _diagnosticSource.Write(_une, new { httpContext = context, exception = ex });
                        return;
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogError(ex2, "Error generating responses.");
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Catch the exceptions before fall into the status code page middleware.
        /// </summary>
        /// <param name="app">The <see cref=" IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> to configure more.</returns>
        public static IApplicationBuilder UseCatchException(this IApplicationBuilder app)
        {
            app.UseMiddleware<CatchExceptionMiddleware>();
            return app;
        }

        /// <summary>
        /// Adds a StatusCodePages middleware with the specified handler that checks for
        /// responses with status codes between 400 and 599 that do not have a body.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> to configure more.</returns>
        public static IApplicationBuilder UseStatusCodePage(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            var matcher = app.ApplicationServices.GetRequiredService<ReExecuteEndpointMatcher>();

            return app.UseStatusCodePages(async (StatusCodeContext context) =>
            {
                if (context.HttpContext.Request.Headers.TryGetValue("X-Requested-With", out var s)
                    && s.First() == "XMLHttpRequest")
                    return;

                if (context.HttpContext.Features.Get<IClaimedNoStatusCodePageFeature>() != null)
                    return;

                if (context.HttpContext.Items.ContainsKey("AuditlogType"))
                    context.HttpContext.Items.Remove("AuditlogType");

                var originalPath = context.HttpContext.Request.Path;
                var originalQueryString = context.HttpContext.Request.QueryString;

                // Store the original paths so the app can check it.
                context.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(new StatusCodeReExecuteFeature
                {
                    OriginalPathBase = context.HttpContext.Request.PathBase.Value,
                    OriginalPath = originalPath.Value,
                    OriginalQueryString = originalQueryString.HasValue ? originalQueryString.Value : null,
                });

                context.HttpContext.SetEndpoint(endpoint: null);
                await matcher.MatchAsync(context.HttpContext);
                var handler = context.HttpContext.GetEndpoint();

                if (handler == null)
                    return;

                await context.Next(context.HttpContext);
            });
        }
    }
}
