using System.Collections.Generic;

namespace Microsoft.AspNetCore.Authentication.EasyAuth
{
    public class EasyAuthAuthenticationOptions : AuthenticationSchemeOptions
    {
        public IReadOnlyList<EasyAuthClientPrincipal.UserClaim>? DeveloperMode { get; set; }

        public string? LoginUrl { get; set; }

        public string? LogoutUrl { get; set; }

        public bool UseHttp302ForChallenge { get; set; }

        public EasyAuthAuthenticationOptions()
        {
            Events = new object();
        }
    }
}
