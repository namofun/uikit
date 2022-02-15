using System;

namespace Microsoft.AspNetCore.Authorization
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class AuthenticateWithAllSchemesAttribute : AuthorizeAttribute
    {
        public void UpdateAuthenticationSchemes(string acceptableSchemes)
        {
            AuthenticationSchemes = string.IsNullOrWhiteSpace(acceptableSchemes) ? null : acceptableSchemes;
        }
    }
}
