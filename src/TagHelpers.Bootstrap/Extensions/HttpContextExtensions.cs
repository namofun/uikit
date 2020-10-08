using System;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// Extensions for HttpContext in Substrate.
    /// </summary>
    public static class SubstrateHttpContextExtensions
    {
        /// <summary>
        /// Redirect to an URL when using ajax request.
        /// </summary>
        /// <param name="httpResponse">The <see cref="HttpResponse"/>.</param>
        /// <param name="redirectUrl">The URL to redirect to.</param>
        /// <param name="statusCode">The status code.</param>
        public static void RedirectAjax(this HttpResponse httpResponse, string redirectUrl, int? statusCode = null)
        {
            httpResponse.Headers["X-Login-Page"] = redirectUrl;
            if (statusCode.HasValue)
                httpResponse.StatusCode = statusCode.Value;
        }

        /// <summary>
        /// Check whether this HttpRequest is ajax request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/>.</param>
        /// <returns>Whether this request is ajax.</returns>
        public static bool IsAjax(this HttpRequest request)
        {
            return string.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal)
                || string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);
        }
    }
}
