using Microsoft.AspNetCore.Identity;
using SatelliteSite.IdentityModule.Services;

namespace SatelliteSite.IdentityModule.Entities
{
    /// <summary>
    /// Represents a role in the identity system.
    /// </summary>
    public class Role : IdentityRole<int>, IRole
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Role"/>.
        /// </summary>
        public Role() { }

        /// <summary>
        /// Initializes a new instance of <see cref="Role"/>.
        /// </summary>
        public Role(string roleName) : base(roleName) { }

        /// <summary>
        /// The short name of the role.
        /// </summary>
        public virtual string ShortName { get; set; }

        /// <summary>
        /// The description of the role.
        /// </summary>
        public virtual string Description { get; set; }
    }
}
