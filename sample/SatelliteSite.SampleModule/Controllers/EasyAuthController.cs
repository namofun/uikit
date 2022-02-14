using Microsoft.AspNetCore.Authentication.EasyAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace SatelliteSite.SampleModule.Controllers
{
    [Authorize]
    [Area("Sample")]
    [Route("[area]/[controller]/[action]")]
    [AuditPoint(AuditlogType.User)]
    public class EasyAuthController : ViewControllerBase
    {
        private readonly EasyAuthAuthenticationOptions _options;

        public EasyAuthController(IOptionsMonitor<EasyAuthAuthenticationOptions> options)
        {
            _options = options.Get("EasyAuth");
        }


        [HttpGet(Name = "EasyAuthLogin")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            return Redirect(_options.LoginUrl + "?post_login_redirect_uri=" + UrlEncoder.Default.Encode(returnUrl ?? "/"));
        }


        [HttpPost(Name = "EasyAuthLogout")]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            return Redirect(_options.LogoutUrl + "?post_logout_redirect_uri=" + UrlEncoder.Default.Encode("/"));
        }
    }
}
