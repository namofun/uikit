using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// A base class for an MVC controller with view and ajax support.
    /// </summary>
    public abstract class Controller2 : Controller
    {
        /// <summary>
        /// Gets or sets the message to display on the page.
        /// </summary>
        [TempData]
        public string? StatusMessage { get; set; }

        /// <summary>
        /// Gets whether this request is sent from <c>XMLHttpRequest</c> by AJAX.
        /// </summary>
        public bool InAjax { get; private set; }

        /// <summary>
        /// Gets whether this request is sent from <c>XMLHttpRequest</c> and has <c>handlekey</c> token.
        /// </summary>
        public bool IsWindowAjax { get; private set; }


        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            string? handlekey = null;
            if (context.HttpContext.Request.Query.TryGetValue(nameof(handlekey), out var qs))
                handlekey = qs.FirstOrDefault();
            else if (context.HttpContext.Request.HasFormContentType
                    && context.HttpContext.Request.Form.TryGetValue(nameof(handlekey), out qs))
                handlekey = qs.FirstOrDefault();

            IsWindowAjax = handlekey != null;

            if (IsWindowAjax || (HttpContext.Request.Headers.TryGetValue("X-Requested-With", out var val)
                    && val.FirstOrDefault() == "XMLHttpRequest"))
                InAjax = true;

            if (InAjax)
            {
                ViewData["InAjax"] = true;
            }
            else
            {
                ViewData["RefreshUrl"] = HttpContext.Request.Path.Value +
                    HttpContext.Request.QueryString.Value.Replace("&amp;", "&");
            }

            if (IsWindowAjax)
            {
                ViewData["HandleKey"] = handlekey;
                ViewData["HandleKey2"] = System.Guid.NewGuid().ToString().Substring(0, 6);
            }
        }


        /// <inheritdoc />
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            if (context.Result is RedirectToActionResult rtas)
                context.Result = new RedirectToAction2Result(rtas, InAjax);

            if (context.Result is ForbidResult && InAjax)
            {
                Response.StatusCode = 403;
                if (IsWindowAjax)
                    context.Result = Message("Access denined", "You do not have access to this resource.", BootstrapColor.danger);
                else
                    context.Result = new EmptyResult();
            }
        }


        /// <summary>
        /// Creates a <see cref="ShowMessageResult"/> object that renders a message to the response.
        /// </summary>
        /// <param name="title">The message title to display</param>
        /// <param name="message">The message to display</param>
        /// <param name="type">The bootstrap dialog button color</param>
        /// <returns>The created <see cref="ShowMessageResult"/> object for the response.</returns>
        [NonAction]
        public ShowMessageResult Message(string title, string message, BootstrapColor type = BootstrapColor.primary)
        {
            return new ShowMessageResult
            {
                ViewData = ViewData,
                TempData = TempData,
                Title = title,
                Content = message,
                Type = type,
            };
        }


        /// <summary>
        /// Creates a <see cref="AskPostResult"/> object that renders action confirmation to the response.
        /// </summary>
        /// <param name="title">The message title to display</param>
        /// <param name="message">The message to display</param>
        /// <param name="area">The area name used to generate links</param>
        /// <param name="controller">The controller name used to generate links</param>
        /// <param name="action">The action name used to generate links</param>
        /// <param name="routeValues">The route values used to generate links</param>
        /// <param name="type">The bootstrap dialog button color</param>
        /// <returns>The created <see cref="AskPostResult"/> object for the response.</returns>
        [NonAction]
        public AskPostResult AskPost(string title, string message, string area, string controller, string action, object? routeValues = null, BootstrapColor type = BootstrapColor.primary)
        {
            if (!(routeValues is Dictionary<string, string> rvd))
            {
                if (routeValues == null)
                {
                    rvd = new Dictionary<string, string>();
                }
                else
                {
                    var vtype = routeValues.GetType();
                    if ((!vtype.FullName?.StartsWith("<>f__AnonymousType")) ?? true)
                        throw new System.ArgumentException(nameof(routeValues));

                    rvd = vtype.GetProperties().ToDictionary(
                        keySelector: p => p.Name,
                        elementSelector: p => $"{p.GetValue(routeValues)}");
                }
            }

            return new AskPostResult
            {
                ViewData = ViewData,
                TempData = TempData,
                Title = title,
                Content = message,
                Type = type,
                AreaName = area,
                ControllerName = controller,
                ActionName = action,
                RouteValues = rvd,
            };
        }


        /// <summary>
        /// Creates a <see cref="AskPostResult"/> object that renders action confirmation to the response.
        /// The action, controller, area parameters are used by this action default.
        /// </summary>
        /// <param name="title">The message title to display</param>
        /// <param name="message">The message to display</param>
        /// <param name="routeValues">The route values used to generate links</param>
        /// <param name="type">The bootstrap dialog button color</param>
        /// <returns>The created <see cref="AskPostResult"/> object for the response.</returns>
        [NonAction]
        public AskPostResult AskPost(string title, string message, object? routeValues = null, BootstrapColor type = BootstrapColor.primary)
        {
            string area, controller, action;
            area = (string)RouteData.Values[nameof(area)];
            controller = (string)RouteData.Values[nameof(controller)];
            action = (string)RouteData.Values[nameof(action)];
            return AskPost(title, message, area, controller, action, routeValues, type);
        }


        /// <summary>
        /// Creates a <see cref="ShowWindowResult"/> object that renders a view to the response.
        /// </summary>
        /// <returns>The created <see cref="ShowWindowResult"/> object for the response.</returns>
        [NonAction]
        public ShowWindowResult Window()
        {
            return Window(null, null);
        }


        /// <summary>
        /// Creates a <see cref="ShowWindowResult"/> object by specifying a <paramref name="model"/>
        /// to be rendered by the view.
        /// </summary>
        /// <param name="model">The model that is rendered by the view.</param>
        /// <returns>The created <see cref="ShowWindowResult"/> object for the response.</returns>
        [NonAction]
        public ShowWindowResult Window(object model)
        {
            return Window(null, model);
        }


        /// <summary>
        /// Creates a <see cref="ShowWindowResult"/> object by specifying a <paramref name="viewName"/>.
        /// </summary>
        /// <param name="viewName">The name or path of the view that is rendered to the response.</param>
        /// <returns>The created <see cref="ShowWindowResult"/> object for the response.</returns>
        [NonAction]
        public ShowWindowResult Window(string viewName)
        {
            return Window(viewName, null);
        }


        /// <summary>
        /// Creates a <see cref="ShowWindowResult"/> object by specifying a <paramref name="viewName"/>
        /// and the <paramref name="model"/> to be rendered by the view.
        /// </summary>
        /// <param name="viewName">The name or path of the view that is rendered to the response.</param>
        /// <param name="model">The model that is rendered by the view.</param>
        /// <returns>The created <see cref="ShowWindowResult"/> object for the response.</returns>
        [NonAction]
        public ShowWindowResult Window(string? viewName, object? model)
        {
            ViewData.Model = model;

            return new ShowWindowResult
            {
                ViewName = viewName,
                ViewData = ViewData,
                TempData = TempData,
            };
        }


        /// <summary>
        /// Creates a <see cref="HttpStatusCode"/> view page by specifying a <paramref name="statusCode"/>.
        /// </summary>
        /// <param name="statusCode">The status code</param>
        /// <returns>A <see cref="ActionResult"/> representing the action result.</returns>
        [NonAction]
        public ActionResult StatusCodePage(int? statusCode = null)
        {
            ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var code = statusCode ?? Response.StatusCode;
            Response.StatusCode = code;
            ViewBag.StatusCode = code;
            
            if (InAjax)
                return Content(
                    $"Sorry, an error has occured: {Regex.Replace(((HttpStatusCode)code).ToString(), "([a-z])([A-Z])", "$1 $2")}.\n" +
                    "Please contact a staff member for assistance.", "text/plain");
            else
                return View("Error");
        }
    }
}
