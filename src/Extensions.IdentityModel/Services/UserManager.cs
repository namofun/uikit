using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    /// <inheritdoc cref="UserManagerBase{TUser,TRole}" />
    public class UserManager<TUser, TRole> :
        UserManagerBase<TUser, TRole>
        where TUser : SatelliteSite.IdentityModule.Entities.User, new()
        where TRole : SatelliteSite.IdentityModule.Entities.Role, new()
    {
        /// <summary>
        /// Construct a new instance of <see cref="UserManager{TUser,TRole}"/>.
        /// </summary>
        public UserManager(
            IUserStore<TUser> store,
            IRoleStore<TRole> roleStore,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUser> passwordHasher,
            IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<TUser, TRole>> logger,
            SlideExpirationService slideExpirationService,
            IOptions<IdentityAdvancedOptions> advOptions)
            : base(store,
                  roleStore,
                  optionsAccessor,
                  passwordHasher,
                  userValidators,
                  passwordValidators,
                  keyNormalizer,
                  errors,
                  services,
                  logger)
        {
            SlideExpirationStore = slideExpirationService;
            SupportsUserTwoFactor = SupportsUserTwoFactorRecoveryCodes = advOptions.Value.TwoFactorAuthentication;
            SupportsUserLogin = advOptions.Value.ExternalLogin;
        }

        /// <summary>
        /// Gets the database context for this store.
        /// </summary>
        private IdentityDbContext<TUser, TRole, int> Context => ((IContextedStore<TUser, TRole>)Store).Context;

        /// <summary>
        /// Gets the expiration for this store.
        /// </summary>
        private SlideExpirationService SlideExpirationStore { get; }

        /// <inheritdoc />
        public override bool SupportsUserTwoFactor { get; }

        /// <inheritdoc />
        public override bool SupportsUserTwoFactorRecoveryCodes { get; }

        /// <inheritdoc />
        public override bool SupportsUserLogin { get; }

        /// <inheritdoc />
        public override Task<IdentityResult> SlideExpirationAsync(TUser user)
        {
            SlideExpirationStore.Set(user.UserName);
            return Task.FromResult(IdentityResult.Success);
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyList<IRole>> ListRolesAsync(TUser user)
        {
            var query = from userRole in Context.UserRoles
                        join role in Context.Roles on userRole.RoleId equals role.Id
                        where userRole.UserId.Equals(user.Id)
                        select role;
            return await query.ToListAsync();
        }

        /// <inheritdoc />
        public override async Task<IPagedList<IUser>> ListUsersAsync(int page, int pageCount)
        {
            return await Users.OrderBy(u => u.Id).ToPagedListAsync(page, pageCount);
        }

        /// <inheritdoc />
        public override async Task<ILookup<int, int>> ListUserRolesAsync(int minUid, int maxUid)
        {
            var lst = await Context.UserRoles
                .Where(ur => ur.UserId >= minUid && ur.UserId <= maxUid)
                .ToListAsync();
            return lst.ToLookup(a => a.UserId, a => a.RoleId);
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyDictionary<int, IRole>> ListNamedRolesAsync()
        {
            var roles = await Context.Roles.Where(r => r.ShortName != null).ToListAsync();
            return roles.AsEnumerable<IRole>().ToDictionary(r => r.Id);
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyList<string>> ListSubscribedEmailsAsync()
        {
            return await Users
                .Where(u => u.EmailConfirmed && u.SubscribeNews)
                .OrderBy(u => u.Id)
                .Select(u => u.Email)
                .ToListAsync();
        }

        /// <inheritdoc />
        public override Task<bool> RoleExistsAsync(string roleName)
        {
            ThrowIfDisposed();
            roleName = NormalizeName(roleName);
            return Context.Roles.Where(r => r.NormalizedName == roleName).AnyAsync();
        }

        /// <inheritdoc />
        public override Task BatchLockOutAsync(IEnumerable<int> query)
        {
            ThrowIfDisposed();
            return Context.Users
                .Where(u => query.Contains(u.Id))
                .BatchUpdateAsync(u => new TUser { LockoutEnd = DateTimeOffset.MaxValue });
        }

        /// <inheritdoc />
        public override Task<Dictionary<int, string>> FindUserNamesAsync(IEnumerable<int> userIds)
        {
            ThrowIfDisposed();
            return Context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(a => a.Id, a => a.UserName);
        }
    }
}
