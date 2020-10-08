using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace SatelliteSite.IdentityModule.Services
{
    public class CookieAuthenticationValidator : CookieAuthenticationEvents
    {
        public CookieAuthenticationValidator()
        {
            OnValidatePrincipal = ValidatePrincipalImpl;
            OnRedirectToLogin = c => RedirectImpl(c, 401);
            OnRedirectToAccessDenied = c => RedirectImpl(c, 403);
            OnRedirectToLogout = c => RedirectImpl(c, null);
            OnRedirectToReturnUrl = c => RedirectImpl(c, null);
        }

        internal readonly static IMemoryCache _cache =
            new MemoryCache(new MemoryCacheOptions()
            {
                Clock = new Microsoft.Extensions.Internal.SystemClock()
            });

        private static async Task ValidatePrincipalImpl(CookieValidatePrincipalContext context)
        {
            var userName = context.Principal.GetUserName();
            DateTimeOffset? orig = context.Properties.IssuedUtc;
            userName = userName?.Normalize().ToUpperInvariant();

            if (userName != null &&
                _cache.TryGetValue("SlideExpiration: " + userName, out DateTimeOffset last) &&
                last > context.Properties.IssuedUtc)
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
