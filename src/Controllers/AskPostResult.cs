using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Represents an <see cref="ActionResult"/> that renders a ask post box to the response.
    /// </summary>
    public class AskPostResult : ViewResult
    {
        /// <summary>
        /// Gets or sets the title to display in the message.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the content to display in the message.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the route data to use for generating the URL.
        /// </summary>
        public Dictionary<string, string>? RouteValues { get; set; }

        /// <summary>
        /// Gets or sets the name of the action to use for generating the URL.
        /// </summary>
        public string? ActionName { get; set; }

        /// <summary>
        /// Gets or sets the name of the controller to use for generating the URL.
        /// </summary>
        public string? ControllerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the area to use for generating the URL.
        /// </summary>
        public string? AreaName { get; set; }

        /// <summary>
        /// Gets or sets the color to use for displaying window.
        /// </summary>
        public BootstrapColor Type { get; set; }

        /// <inheritdoc />
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (ViewData.ContainsKey("InAjax") && !ViewData.ContainsKey("HandleKey"))
                throw new InvalidOperationException();
            ViewData["Message"] = Content;
            ViewData["Title"] = Title;
            ViewData["RouteValues"] = RouteValues ?? new Dictionary<string, string>();
            ViewData["AreaName"] = AreaName;
            ViewData["ControllerName"] = ControllerName;
            ViewData["ActionName"] = ActionName;
            ViewData["MsgType"] = Type.ToString().ToLower();
            ViewName = "_AskForPost";

            return base.ExecuteResultAsync(context);
        }
    }
}
