using System;

namespace SatelliteSite.IdentityModule.Services
{
    public interface IUser
    {
        /// <summary>
        /// Gets or sets the date and time, in UTC, when any user lockout ends.
        /// </summary>
        /// <remarks>A value in the past means the user is not locked out.</remarks>
        DateTimeOffset? LockoutEnd { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if two factor authentication is enabled for this user.
        /// </summary>
        /// <value>True if 2fa is enabled, otherwise false.</value>
        bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// Gets or sets the email address for this user.
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Check the <paramref name="username"/> is the same as NormalizedUserName.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <returns>Whether this user matches this user name.</returns>
        bool HasUserName(string username);

        /// <summary>
        /// Checks whether the user has a password.
        /// </summary>
        /// <returns>Whether the user has a password.</returns>
        bool HasPassword();

        /// <summary>
        /// Gets or sets a flag indicating if a user has confirmed their email address.
        /// </summary>
        /// <value>True if the email address has been confirmed, otherwise false.</value>
        bool EmailConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the user name for this user.
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// A random value that must change whenever a users credentials change (password changed, login removed)
        /// </summary>
        string SecurityStamp { get; }

        /// <summary>
        /// Gets the primary key for this user.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets or sets a flag indicating if the user could be locked out.
        /// </summary>
        /// <value>True if the user could be locked out, otherwise false.</value>
        bool LockoutEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets the number of failed login attempts for the current user.
        /// </summary>
        int AccessFailedCount { get; set; }

        /// <summary>
        /// Gets or sets the nickname of user.
        /// </summary>
        string NickName { get; set; }

        /// <summary>
        /// Gets or sets the personal signature of user.
        /// </summary>
        string Plan { get; set; }

        /// <summary>
        /// Gets or sets the register time of user.
        /// </summary>
        DateTimeOffset? RegisterTime { get; set; }

        /// <summary>
        /// Gets or sets whether this user subscribe news.
        /// </summary>
        bool SubscribeNews { get; set; }
    }
}