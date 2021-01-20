using System.Linq;
using System.Security.Claims;

namespace System
{
    /// <summary>
    /// Claims related extensions for <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public static class UserClaimsPrincipalExtensions
    {
        public const string ClaimTypes_Name = "name";
        public const string ClaimTypes_Role = "role";
        public const string ClaimTypes_NameIdentifier = "sub";
        public const string ClaimTypes_NickName = "nickname";

        /// <summary>
        /// Returns the value for the first claim of the specified type otherwise null the claim is not present.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> instance this method extends.</param>
        /// <param name="claimType">The claim type whose first value should be returned.</param>
        /// <returns>The value of the first instance of the specified claim type, or null if the claim is not present.</returns>
        private static string? FindFirstValue(this ClaimsPrincipal principal, string claimType)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            return principal.FindFirst(claimType)?.Value;
        }

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
    }
}
