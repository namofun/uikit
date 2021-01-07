using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.Identity
{
    /// <summary>
    /// The user store interface containing the DbContext.
    /// </summary>
    public interface IContextedStore<TUser, TRole>
        where TUser : SatelliteSite.IdentityModule.Entities.User
        where TRole : SatelliteSite.IdentityModule.Entities.Role
    {
        /// <summary>
        /// Gets the database context for this store.
        /// </summary>
        IdentityDbContext<TUser, TRole, int> Context { get; }
    }

    /// <summary>
    /// Represents a new instance of a persistence store for the specified user and role types.
    /// </summary>
    /// <typeparam name="TUser">The type representing a user.</typeparam>
    /// <typeparam name="TRole">The type representing a role.</typeparam>
    /// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
    public class UserStore<TUser, TRole, TContext> :
        UserStore<
            TUser, TRole, TContext, int,
            IdentityUserClaim<int>,
            IdentityUserRole<int>,
            IdentityUserLogin<int>,
            IdentityUserToken<int>,
            IdentityRoleClaim<int>>,
        IContextedStore<TUser, TRole>
        where TUser : SatelliteSite.IdentityModule.Entities.User
        where TRole : SatelliteSite.IdentityModule.Entities.Role
        where TContext : IdentityDbContext<TUser, TRole, int>
    {
        /// <summary>
        /// Creates a new instance of the store.
        /// </summary>
        /// <param name="context">The context used to access the store.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
        public UserStore(TContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
        }

        /// <inheritdoc />
        IdentityDbContext<TUser, TRole, int> IContextedStore<TUser, TRole>.Context => Context;
    }

    /// <summary>
    /// Creates a new instance of a persistence store for roles.
    /// </summary>
    /// <typeparam name="TRole">The type of the class representing a role.</typeparam>
    /// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
    public class RoleStore<TRole, TContext> :
        RoleStore<
            TRole, TContext, int,
            IdentityUserRole<int>,
            IdentityRoleClaim<int>>
        where TRole : SatelliteSite.IdentityModule.Entities.Role
        where TContext : DbContext
    {
        /// <summary>
        /// Creates a new instance of the store.
        /// </summary>
        /// <param name="context">The context used to access the store.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
        public RoleStore(TContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
        }
    }
}
