using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An <see cref="ActionResult"/> that returns a Found (302), Moved Permanently
    /// (301), Temporary Redirect (307), or Permanent Redirect (308) response with a
    /// <c>Location</c> header. Targets a certain URL. If ajax is used, set the
    /// <c>X-Login-Page</c> header for javascript to handle the behavior.
    /// </summary>
    internal class Redirect2Result : ActionResult, IKeepTempDataResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToAction2Result"/>
        /// with the values provided.
        /// </summary>
        /// <param name="src">The original <see cref="RedirectToActionResult"/></param>
        /// <param name="inajax">Whether this action is in ajax</param>
        public Redirect2Result(RedirectResult src, bool inajax)
        {
            InAjax = inajax;
            Url = src.Url;
            PreserveMethod = src.PreserveMethod;
        }

        /// <summary>
        /// Gets or sets the URL to redirect to.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets an indication that the redirect preserves the initial request method.
        /// </summary>
        public bool PreserveMethod { get; set; }

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

            if (InAjax)
            {
                context.HttpContext.Response.StatusCode =
                    StatusCodes.Status200OK;
                context.HttpContext.Response.Headers["X-Login-Page"] = Url;
            }
            else
            {
                context.HttpContext.Response.StatusCode = PreserveMethod
                    ? StatusCodes.Status307TemporaryRedirect
                    : StatusCodes.Status302Found;
                context.HttpContext.Response.Headers[HeaderNames.Location] = Url;
            }
        }
    }
}
