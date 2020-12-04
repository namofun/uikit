using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Represents an <see cref="ActionResult"/> that renders a window to the response.
    /// </summary>
    /// <remarks>
    /// <para>There's several useful <c>ViewData</c> keys for modal views.</para>
    /// <list type="bullet"><c>FormAjaxUpload</c>: Enable ajaxupload feature.</list>
    /// <list type="bullet"><c>MaxWidth</c>: Change the max modal width.</list>
    /// <list type="bullet"><c>IgnoreCancelButton</c>: Do not show the cancel button.</list>
    /// <list type="bullet"><c>StaticBackdrop</c>: Set the backdrop as static and ignore keyboard ESC.</list>
    /// </remarks>
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
