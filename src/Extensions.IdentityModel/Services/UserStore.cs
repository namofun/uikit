using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
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
        IUserListStore<TUser, TRole>
        where TUser : SatelliteSite.IdentityModule.Entities.User, new()
        where TRole : SatelliteSite.IdentityModule.Entities.Role, new()
        where TContext : IdentityDbContext<TUser, TRole, int>
    {
        /// <summary>
        /// A navigation property for the roles the store contains.
        /// </summary>
        public IQueryable<TRole> Roles => Context.Roles;

        /// <summary>
        /// A navigation property for the role claims the store contains.
        /// </summary>
        public IQueryable<IdentityRoleClaim<int>> RoleClaims => Context.RoleClaims;

        /// <summary>
        /// A navigation property for the user-role relations the store contains.
        /// </summary>
        public IQueryable<IdentityUserRole<int>> UserRoles => Context.UserRoles;

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
        public Task<IPagedList<TUser>> ListAsync(int page, int pageCount, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Users
                .OrderBy(u => u.Id)
                .ToPagedListAsync(page, pageCount, cancellationToken);
        }

        /// <inheritdoc />
        public Task<List<TRole>> ListRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return UserRoles
                .Where(ur => ur.UserId.Equals(user.Id))
                .Join(Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task<List<TRole>> ListNamedRolesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Roles
                .Where(r => r.ShortName != null)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task<ILookup<int, int>> ListUserRolesAsync(int minUserId, int maxUserId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return UserRoles
                .Where(ur => ur.UserId >= minUserId && ur.UserId <= maxUserId)
                .ToLookupAsync(a => a.UserId, a => a.RoleId);
        }

        /// <inheritdoc />
        public Task<List<string>> ListSubscribedEmailsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Users
                .Where(u => u.EmailConfirmed && u.SubscribeNews)
                .OrderBy(u => u.Id)
                .Select(u => u.Email)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task<bool> ExistRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Roles
                .Where(r => r.NormalizedName == normalizedRoleName)
                .AnyAsync();
        }

        /// <inheritdoc />
        public Task<int> LockOutUsersAsync(IEnumerable<int> userIds, string newSecurityStamp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Users
                .Where(u => userIds.Contains(u.Id))
                .BatchUpdateAsync(u => new TUser { LockoutEnd = DateTimeOffset.MaxValue, SecurityStamp = newSecurityStamp }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Dictionary<int, string>> ListUserNamesAsync(IEnumerable<int> userIds, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(a => a.Id, a => a.UserName, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<List<Claim>> ListUserRoleClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var claims = new List<Claim>();
            var query = UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(RoleClaims, ur => ur.RoleId, rc => rc.RoleId, (ur, rc) => rc)
                .Select(rc => new { rc.ClaimType, rc.ClaimValue })
                .Distinct();

            await foreach (var claim in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                claims.Add(new Claim(claim.ClaimType, claim.ClaimValue));
            }

            return claims;
        }
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
