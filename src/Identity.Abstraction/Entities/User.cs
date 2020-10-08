using Microsoft.AspNetCore.Identity;
using SatelliteSite.IdentityModule.Services;
using System;

namespace SatelliteSite.IdentityModule.Entities
{
    /// <summary>
    /// Represents a user in the identity system.
    /// </summary>
    public class User : IdentityUser<int>, IUser
    {
        /// <summary>
        /// Initializes a new instance of <see cref="User"/>.
        /// </summary>
        public User() { }

        /// <summary>
        /// Initializes a new instance of <see cref="User"/>.
        /// </summary>
        public User(string userName) : base(userName) { }

        /// <inheritdoc />
        public virtual string NickName { get; set; }

        /// <inheritdoc />
        public virtual string Plan { get; set; }

        /// <inheritdoc />
        public virtual DateTimeOffset? RegisterTime { get; set; }

        /// <inheritdoc />
        public virtual bool SubscribeNews { get; set; } = true;

        /// <inheritdoc />
        public bool HasPassword()
        {
            return PasswordHash != null;
        }

        /// <inheritdoc />
        public bool HasUserName(string username)
        {
            return NormalizedUserName == username.ToUpper();
        }
    }
}
