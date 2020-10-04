using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SatelliteSite.IdentityModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SatelliteSite.IdentityModule.Services
{
    /// <inheritdoc cref="UserManager{TUser}" />
    public abstract class UserManagerBase<TUser, TRole> : UserManager<TUser>
        where TUser : User
        where TRole : Role
    {
        /// <summary>
        /// Construct a new instance of <see cref="UserManagerBase{TUser,TRole}"/>.
        /// </summary>
        public UserManagerBase(
            IUserStore<TUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUser> passwordHasher,
            IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManagerBase<TUser, TRole>> logger)
            : base(store,
                  optionsAccessor,
                  passwordHasher,
                  userValidators,
                  passwordValidators,
                  keyNormalizer,
                  errors,
                  services,
                  logger)
        {
        }

        /// <inheritdoc />
        public override string GetUserId(ClaimsPrincipal principal)
        {
            // Be careful of the old logins
            return base.GetUserId(principal)
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <inheritdoc />
        public virtual string GetNickName(ClaimsPrincipal claim)
        {
            var nickName = claim.FindFirstValue("nickname");
            if (string.IsNullOrEmpty(nickName)) nickName = GetUserName(claim);
            return nickName;
        }

        /// <summary>
        /// Mark the user's information as slide expiration so that the user claims will be re-generated.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <returns>The operation result.</returns>
        public abstract Task<IdentityResult> SlideExpirationAsync(TUser user);

        /// <inheritdoc />
        public override async Task<IdentityResult> AddToRoleAsync(TUser user, string role)
        {
            var result = await base.AddToRoleAsync(user, role);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <inheritdoc />
        public override async Task<IdentityResult> AddToRolesAsync(TUser user, IEnumerable<string> roles)
        {
            var result = await base.AddToRolesAsync(user, roles);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <inheritdoc />
        public override async Task<IdentityResult> RemoveFromRoleAsync(TUser user, string role)
        {
            var result = await base.RemoveFromRoleAsync(user, role);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <inheritdoc />
        public override async Task<IdentityResult> RemoveFromRolesAsync(TUser user, IEnumerable<string> roles)
        {
            var result = await base.RemoveFromRolesAsync(user, roles);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <inheritdoc />
        public override async Task<IdentityResult> ConfirmEmailAsync(TUser user, string token)
        {
            var result = await base.ConfirmEmailAsync(user, token);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <summary>
        /// Finds and returns a user, if any, who has the specified <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The user ID to search for.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="userId"/> if it exists.</returns>
        public virtual Task<TUser> FindByIdAsync(int userId)
        {
            return FindByIdAsync($"{userId}");
        }

        /// <summary>
        /// List roles of users.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The task for fetching role list.</returns>
        public abstract Task<IReadOnlyList<IRole>> ListRolesAsync(TUser user);

        /// <summary>
        /// Gets the paged list of users.
        /// </summary>
        /// <param name="page">The page id.</param>
        /// <param name="pageCount">The count per page.</param>
        /// <returns>The task for fetching user list.</returns>
        public abstract Task<IPagedList<IUser>> ListUsersAsync(int page, int pageCount);

        /// <summary>
        /// Gets the paged list of user roles.
        /// </summary>
        /// <param name="minUid">The minimum user ID.</param>
        /// <param name="maxUid">The maximum user ID.</param>
        /// <returns>The task for fetching user role list.</returns>
        public abstract Task<ILookup<int, int>> ListUserRolesAsync(int minUid, int maxUid);

        /// <summary>
        /// Gets the dictionary of named roles.
        /// </summary>
        /// <returns>The task for fetching role dictionary.</returns>
        public abstract Task<IReadOnlyDictionary<int, IRole>> ListNamedRolesAsync();

        /// <summary>
        /// Gets the mail list of users with subscription.
        /// </summary>
        /// <returns>The task for fetching user list.</returns>
        public abstract Task<IReadOnlyList<string>> ListSubscribedEmailsAsync();
    }
}
