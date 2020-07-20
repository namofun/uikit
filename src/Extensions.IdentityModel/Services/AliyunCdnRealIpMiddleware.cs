using System.Net;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// A middleware that solves header <c>Ali-Cdn-Real-Ip</c>.
    /// </summary>
    public class AliyunCdnRealIpMiddleware
    {
        private readonly RequestDelegate _next;

        public AliyunCdnRealIpMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            var headers = context.Request.Headers;

            if (headers.ContainsKey("Ali-Cdn-Real-Ip"))
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse(headers["Ali-Cdn-Real-Ip"]);
            }

            return _next(context);
        }
    }
}
