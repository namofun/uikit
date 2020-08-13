﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
    public static class ClaimsPrincipalExtensions
    {
        private const string ClaimTypes_Name = "name";
        private const string ClaimTypes_NameIdentifier = "sub";
        private const string ClaimTypes_NickName = "nickname";

        /// <summary>
        /// Whether this <see cref="ClaimsPrincipal"/> joined the following roles.
        /// </summary>
        /// <param name="user">The principal to check</param>
        /// <param name="roles">The roles to check, seperated by single <c>,</c>s.</param>
        /// <returns><c>true</c> if this user belongs to any of these roles</returns>
        public static bool IsInRoles(this ClaimsPrincipal user, string roles)
        {
            return roles.Split(',').Any(role => user.IsInRole(role));
        }

        /// <summary>
        /// Get the claimed username from this <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="principal">The principal to check</param>
        /// <returns><c>null</c> if this principal is not logged in</returns>
        public static string? GetUserName(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Name)
                ?? principal.FindFirstValue(ClaimTypes_Name);
        }

        /// <summary>
        /// Get the claimed user id from this <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="principal">The principal to check</param>
        /// <returns><c>null</c> if this principal is not logged in</returns>
        public static string? GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue(ClaimTypes_NameIdentifier);
        }

        /// <summary>
        /// Get the claimed nickname from this <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="principal">The principal to check</param>
        /// <returns><c>null</c> if this principal is not logged in</returns>
        public static string? GetNickName(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes_NickName)
                ?? GetUserName(principal);
        }

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
        /// Make the cookie configuration after <see cref="AuthenticationBuilder"/>.
        /// </summary>
        /// <param name="builder">The builder</param>
        /// <param name="configure">The configuration function</param>
        /// <returns>The original builder</returns>
        public static AuthenticationBuilder SetCookie(
            this AuthenticationBuilder builder,
            Action<CookieAuthenticationOptions> configure)
        {
            builder.Services.ConfigureApplicationCookie(configure);
            return builder;
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
                optionsAccessor.Value.ClaimsIdentity.UserIdClaimType != ClaimTypes_NameIdentifier)
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