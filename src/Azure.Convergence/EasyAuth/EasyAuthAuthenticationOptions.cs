namespace Microsoft.AspNetCore.Authentication.EasyAuth
{
    public class EasyAuthAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string LoginUrl { get; set; } = "/.auth/login/aad";

        public string LogoutUrl { get; set; } = "/.auth/logout";

        public bool UseHttp302ForChallenge { get; set; }

        public EasyAuthAuthenticationOptions()
        {
            Events = new object();
        }
    }
}
