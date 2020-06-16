using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Represents an <see cref="ActionResult"/> that renders a window to the response.
    /// </summary>
    public class ShowWindowResult : ViewResult
    {
        /// <inheritdoc />
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (ViewData.ContainsKey("InAjax") && !ViewData.ContainsKey("HandleKey"))
                throw new InvalidOperationException();
            if (!ViewData.ContainsKey("HandleKey2"))
                ViewData["HandleKey2"] = Guid.NewGuid().ToString("N")[0..8];
            return base.ExecuteResultAsync(context);
        }
    }
}
