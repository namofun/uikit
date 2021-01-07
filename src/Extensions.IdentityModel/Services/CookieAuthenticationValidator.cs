using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication
{
    public class CookieAuthenticationValidator : CookieAuthenticationEvents
    {
        private readonly SlideExpirationService _cache;

        public CookieAuthenticationValidator(SlideExpirationService cache)
        {
            _cache = cache;
            OnValidatePrincipal = ValidatePrincipalImpl;
            OnRedirectToLogin = c => RedirectImpl(c, 401);
            OnRedirectToAccessDenied = c => RedirectImpl(c, 403);
            OnRedirectToLogout = c => RedirectImpl(c, null);
            OnRedirectToReturnUrl = c => RedirectImpl(c, null);
        }

        private async Task ValidatePrincipalImpl(CookieValidatePrincipalContext context)
        {
            var userName = context.Principal.GetUserName();
            DateTimeOffset? orig = context.Properties.IssuedUtc;
            userName = userName?.Normalize().ToUpperInvariant();

            if (userName != null &&
                _cache.Get(userName, context.Properties.IssuedUtc))
                context.Properties.IssuedUtc = null;

            await SecurityStampValidator.ValidatePrincipalAsync(context);
            context.Properties.IssuedUtc = orig;
        }

        private static Task RedirectImpl(RedirectContext<CookieAuthenticationOptions> context, int? statusCode)
        {
            if (context.Request.IsAjax())
                context.Response.RedirectAjax(context.RedirectUri, statusCode);
            else
                context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
    }
}
