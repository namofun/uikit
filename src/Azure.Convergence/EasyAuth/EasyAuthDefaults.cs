using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.EasyAuth;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class EasyAuthDefaults
    {
        public const string AuthenticationScheme = "EasyAuth";

        public static AuthenticationBuilder AddEasyAuth(
            this AuthenticationBuilder builder,
            Action<EasyAuthAuthenticationOptions>? configureOptions = null)
            => builder
                .AddScheme<EasyAuthAuthenticationOptions, EasyAuthAuthenticationHandler>(
                    AuthenticationScheme,
                    AuthenticationScheme,
                    configureOptions);
    }
}
