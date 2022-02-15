using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;

namespace SatelliteSite.IdentityModule
{
    internal class CookieAuthenticateSchemeConfigurator : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly CookieAuthenticationValidator _cookie;

        public CookieAuthenticateSchemeConfigurator(CookieAuthenticationValidator cookie)
        {
            _cookie = cookie;
        }

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            if (name != IdentityConstants.ApplicationScheme) return;

            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.LoginPath = "/account/login";
            options.LogoutPath = "/account/logout";
            options.AccessDeniedPath = "/account/access-denied";
            options.SlidingExpiration = true;
            options.Events.OnValidatePrincipal = _cookie.ValidatePrincipal;
            options.Events.OnRedirectToLogin = _cookie.RedirectToLogin;
            options.Events.OnRedirectToAccessDenied = _cookie.RedirectToAccessDenied;
            options.Events.OnRedirectToLogout = _cookie.RedirectToLogout;
            options.Events.OnRedirectToReturnUrl = _cookie.RedirectToReturnUrl;
        }

        public void Configure(CookieAuthenticationOptions options)
        {
        }
    }

    internal class BasicAuthenticateSchemeConfigurator : IConfigureNamedOptions<BasicAuthenticationOptions>
    {
        private readonly IdentityAdvancedOptions _options;
        private readonly BasicAuthenticationValidator _basic;

        public BasicAuthenticateSchemeConfigurator(IOptions<IdentityAdvancedOptions> options, BasicAuthenticationValidator basic)
        {
            _options = options.Value;
            _basic = basic;
        }

        public void Configure(string name, BasicAuthenticationOptions options)
        {
            if (name != BasicAuthenticationDefaults.AuthenticationScheme) return;

            options.Realm = _options.SiteName;
            options.AllowInsecureProtocol = true;
            options.Events ??= new BasicAuthenticationEvents();
            options.Events.OnValidateCredentials = _basic.ValidateAsync;
        }

        public void Configure(BasicAuthenticationOptions options)
        {
        }
    }
}
