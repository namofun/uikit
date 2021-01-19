﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        public override Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            // fixed in ASP.NET Core 5.0
            return Users.Where(u => u.NormalizedEmail == normalizedEmail).SingleOrDefaultAsync();
        }

        /// <inheritdoc />
        public override async Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var token = await FindTokenAsync(user, loginProvider, name, cancellationToken);
            if (token == null)
            {
                await AddUserTokenAsync(CreateUserToken(user, loginProvider, name, value));
            }
            else
            {
                token.Value = value;

                // Not processed yet
                // https://github.com/dotnet/aspnetcore/issues/29426
                await UpdateUserTokenAsync(token);
            }
        }

        /// <summary>
        /// Remove a new user token.
        /// </summary>
        /// <param name="token">The token to be removed.</param>
        /// <returns></returns>
        protected virtual Task UpdateUserTokenAsync(IdentityUserToken<int> token)
        {
            Context.UserTokens.Update(token);
            return Task.CompletedTask;
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
