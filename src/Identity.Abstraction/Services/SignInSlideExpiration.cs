using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// The service for setting the login slide expiration of some user
    /// so that the role or claims information can be updated quickly
    /// for the user visiting this website.
    /// </summary>
    /// <remarks>This service should be registered as <see cref="ServiceLifetime.Singleton"/>.</remarks>
    public interface ISignInSlideExpiration
    {
        /// <summary>
        /// Marks the user with <see cref="ClaimsPrincipal"/> update needed.
        /// </summary>
        /// <param name="userName">The user name.</param>
        void MarkExpired(string userName);

        /// <summary>
        /// Gets a flag indicating whether the user needs <see cref="ClaimsPrincipal"/> update.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="issuedUtc">The issue time of that principal.</param>
        bool CheckExpired(string userName, DateTimeOffset? issuedUtc);

        /// <summary>
        /// Finds the user with corresponding <paramref name="userName"/>.
        /// </summary>
        /// <param name="serviceProvider">The scoped service provider.</param>
        /// <param name="userName">The user name.</param>
        /// <returns>The task for fetching user.</returns>
        Task<IUser> FindAsync(IServiceProvider serviceProvider, string userName);

        /// <summary>
        /// Verify the password with corresponding <paramref name="user"/>.
        /// </summary>
        /// <param name="serviceProvider">The scoped service provider.</param>
        /// <param name="user">The user entity.</param>
        /// <param name="providedPassword">The provided password.</param>
        /// <returns>The task for fetching user.</returns>
        PasswordVerificationResult VerifyPassword(IServiceProvider serviceProvider, IUser user, string providedPassword);

        /// <summary>
        /// Issue a <see cref="ClaimsPrincipal"/> and keep it in the cache.
        /// </summary>
        /// <param name="serviceProvider">The scoped service provider.</param>
        /// <param name="user">The user entity.</param>
        /// <param name="full">Whether to create a principal will full claims.</param>
        /// <returns>The task for creating the <see cref="ClaimsPrincipal"/>.</returns>
        Task<ClaimsPrincipal> IssueAsync(IServiceProvider serviceProvider, IUser user, bool full);
    }
}
