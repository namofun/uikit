using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An <see cref="ActionResult"/> that returns a Found (302), Moved Permanently
    /// (301), Temporary Redirect (307), or Permanent Redirect (308) response with a
    /// <c>Location</c> header. Targets a controller action. If ajax is used, set the
    /// <c>X-Login-Page</c> header for javascript to handle the behavior.
    /// </summary>
    internal class RedirectToAction2Result : ActionResult, IKeepTempDataResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToAction2Result"/>
        /// with the values provided.
        /// </summary>
        /// <param name="src">The original <see cref="RedirectToActionResult"/></param>
        /// <param name="inajax">Whether this action is in ajax</param>
        public RedirectToAction2Result(RedirectToActionResult src, bool inajax)
        {
            InAjax = inajax;
            ActionName = src.ActionName;
            ControllerName = src.ControllerName;
            RouteValues = src.RouteValues;
            PreserveMethod = src.PreserveMethod;
            Fragment = src.Fragment;
        }

        /// <summary>
        /// Gets or sets the name of the action to use for generating the URL.
        /// </summary>
        public string? ActionName { get; set; }

        /// <summary>
        /// Gets or sets the name of the controller to use for generating the URL.
        /// </summary>
        public string? ControllerName { get; set; }

        /// <summary>
        /// Gets or sets the route data to use for generating the URL.
        /// </summary>
        public RouteValueDictionary? RouteValues { get; set; }

        /// <summary>
        /// Gets or sets an indication that the redirect preserves the initial request method.
        /// </summary>
        public bool PreserveMethod { get; set; }

        /// <summary>
        /// Gets or sets the fragment to add to the URL.
        /// </summary>
        public string? Fragment { get; set; }

        /// <summary>
        /// Gets or sets the action whether in ajax.
        /// </summary>
        public bool InAjax { get; set; }

        /// <inheritdoc />
        public override Task ExecuteResultAsync(ActionContext context)
        {
            ExecuteResult(context);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override void ExecuteResult(ActionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var urlHelper = context.HttpContext.RequestServices
                .GetRequiredService<IUrlHelperFactory>()
                .GetUrlHelper(context);

            var destinationUrl = urlHelper.Action(
                ActionName,
                ControllerName,
                RouteValues,
                protocol: null,
                host: null,
                fragment: Fragment);

            if (string.IsNullOrEmpty(destinationUrl))
                throw new InvalidOperationException("No Routes Matched");

            if (InAjax)
            {
                context.HttpContext.Response.StatusCode =
                    StatusCodes.Status200OK;
                context.HttpContext.Response.Headers["X-Login-Page"] = destinationUrl;
            }
            else
            {
                context.HttpContext.Response.StatusCode = PreserveMethod
                    ? StatusCodes.Status307TemporaryRedirect
                    : StatusCodes.Status302Found;
                context.HttpContext.Response.Headers[HeaderNames.Location] = destinationUrl;
            }
        }
    }
}
