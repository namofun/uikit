using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Represents an <see cref="ActionResult"/> that renders a message box to the response.
    /// </summary>
    public class ShowMessageResult : ViewResult
    {
        /// <summary>
        /// Gets or sets the title to display in the message.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the title to display in the message.
        /// </summary>
        public string? Content { get; set; }

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
            ViewData["MsgType"] = Type.ToString();
            ViewName = "_ShowMessage";

            return base.ExecuteResultAsync(context);
        }
    }
}
