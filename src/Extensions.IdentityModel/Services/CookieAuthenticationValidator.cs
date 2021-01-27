using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication
{
    public class CookieAuthenticationValidator
    {
        private readonly ISignInSlideExpiration _cache;

        public CookieAuthenticationValidator(ISignInSlideExpiration cache)
        {
            _cache = cache;
        }

        public virtual async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userName = context.Principal.GetUserName();
            DateTimeOffset? orig = context.Properties.IssuedUtc;
            userName = userName?.Normalize().ToUpperInvariant();

            if (userName != null &&
                _cache.CheckExpired(userName, context.Properties.IssuedUtc))
                context.Properties.IssuedUtc = null;

            await SecurityStampValidator.ValidatePrincipalAsync(context);
            context.Properties.IssuedUtc = orig;
        }

        protected virtual Task Redirect(RedirectContext<CookieAuthenticationOptions> context, int? statusCode)
        {
            if (context.Request.IsAjax())
                context.Response.RedirectAjax(context.RedirectUri, statusCode);
            else
                context.Response.Redirect(context.RedirectUri);

            return Task.CompletedTask;
        }

        public virtual Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
            => Redirect(context, 403);

        public virtual Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
            => Redirect(context, 401);

        public virtual Task RedirectToLogout(RedirectContext<CookieAuthenticationOptions> context)
            => Redirect(context, null);

        public virtual Task RedirectToReturnUrl(RedirectContext<CookieAuthenticationOptions> context)
            => Redirect(context, null);
    }
}
