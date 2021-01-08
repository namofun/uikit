using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Claims related extensions for <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Check whether this <see cref="ClaimsPrincipal"/> is signed in.
        /// </summary>
        /// <param name="principal">The principal to check</param>
        /// <returns><c>false</c> if this principal is not logged in</returns>
        public static bool IsSignedIn(this ClaimsPrincipal principal)
        {
            return principal?.Identities != null &&
                principal.Identities.Any(i => i.AuthenticationType == IdentityConstants.ApplicationScheme);
        }

        /// <summary>
        /// Make sure the claims types in <see cref="IdentityOptions"/> is correctly set.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The original application builder</returns>
        public static IApplicationBuilder EnsureClaimTypes(this IApplicationBuilder app)
        {
            var optionsAccessor = app.ApplicationServices
                .GetRequiredService<IOptions<IdentityOptions>>();
            if (optionsAccessor.Value.ClaimsIdentity.UserIdClaimType != ClaimTypes.NameIdentifier &&
                optionsAccessor.Value.ClaimsIdentity.UserIdClaimType != UserClaimsPrincipalExtensions.ClaimTypes_NameIdentifier)
                throw new InvalidOperationException(
                    "The UserIdClaimType is not set to either \"sub\" " +
                    "or ClaimTypes.NameIdentifier. " +
                    "Make sure your identity options are correctly set.");
            return app;
        }

        /// <summary>
        /// Get lined error strings from <see cref="ModelStateDictionary"/>.
        /// </summary>
        /// <param name="modelState">The model state dictionary</param>
        /// <returns>The error status</returns>
        public static string GetErrorStrings(this ModelStateDictionary modelState)
        {
            var sb = new StringBuilder();
            foreach (var state in modelState)
            {
                if (state.Value.ValidationState != ModelValidationState.Invalid) continue;
                foreach (var item in state.Value.Errors)
                    sb.AppendLine(item.ErrorMessage);
            }

            return sb.ToString();
        }
    }
}
