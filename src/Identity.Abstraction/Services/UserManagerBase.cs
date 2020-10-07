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
    public abstract class UserManagerBase<TUser, TRole> :
        UserManager<TUser>, IUserManager
        where TUser : User, new()
        where TRole : Role, new()
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

        #region IUser conversion
        Task<IdentityResult> IUserManager.AddToRoleAsync(IUser user, string role) => AddToRoleAsync((TUser)user, role);
        Task<IdentityResult> IUserManager.AddToRolesAsync(IUser user, IEnumerable<string> roles) => AddToRolesAsync((TUser)user, roles);
        Task<bool> IUserManager.IsInRoleAsync(IUser user, string role) => IsInRoleAsync((TUser)user, role);
        Task<IdentityResult> IUserManager.RemoveFromRoleAsync(IUser user, string role) => RemoveFromRoleAsync((TUser)user, role);
        Task<IdentityResult> IUserManager.RemoveFromRolesAsync(IUser user, IEnumerable<string> roles) => RemoveFromRolesAsync((TUser)user, roles);
        Task<IList<string>> IUserManager.GetRolesAsync(IUser user) => GetRolesAsync((TUser)user);
        Task<IReadOnlyList<IRole>> IUserManager.ListRolesAsync(IUser user) => ListRolesAsync((TUser)user);
        async Task<IReadOnlyList<IUser>> IUserManager.GetUsersInRoleAsync(string roleName) => (List<TUser>)await GetUsersInRoleAsync(roleName);
        Task<IdentityResult> IUserManager.DeleteAsync(IUser user) => DeleteAsync((TUser)user);
        async Task<IUser> IUserManager.FindByNameAsync(string userName) => await FindByNameAsync(userName);
        async Task<IUser> IUserManager.FindByEmailAsync(string email) => await FindByEmailAsync(email);
        async Task<IUser> IUserManager.FindByIdAsync(int userId) => await FindByIdAsync($"{userId}");
        Task<IdentityResult> IUserManager.UpdateAsync(IUser user) => UpdateAsync((TUser)user);
        Task<IdentityResult> IUserManager.CreateAsync(IUser user) => CreateAsync((TUser)user);
        Task<IdentityResult> IUserManager.CreateAsync(IUser user, string password) => CreateAsync((TUser)user, password);
        Task<bool> IUserManager.HasPasswordAsync(IUser user) => HasPasswordAsync((TUser)user);
        Task<IdentityResult> IUserManager.AddPasswordAsync(IUser user, string password) => AddPasswordAsync((TUser)user, password);
        Task<IdentityResult> IUserManager.ChangePasswordAsync(IUser user, string currentPassword, string newPassword) => ChangePasswordAsync((TUser)user, currentPassword, newPassword);
        Task<string> IUserManager.GeneratePasswordResetTokenAsync(IUser user) => GeneratePasswordResetTokenAsync((TUser)user);
        Task<IdentityResult> IUserManager.ResetPasswordAsync(IUser user, string token, string newPassword) => ResetPasswordAsync((TUser)user, token, newPassword);
        Task<bool> IUserManager.IsEmailConfirmedAsync(IUser user) => IsEmailConfirmedAsync((TUser)user);
        Task<string> IUserManager.GenerateEmailConfirmationTokenAsync(IUser user) => GenerateEmailConfirmationTokenAsync((TUser)user);
        Task<IdentityResult> IUserManager.ConfirmEmailAsync(IUser user, string token) => ConfirmEmailAsync((TUser)user, token);
        Task<IdentityResult> IUserManager.SetEmailAsync(IUser user, string email) => SetEmailAsync((TUser)user, email);
        async Task<IUser> IUserManager.GetUserAsync(ClaimsPrincipal principal) => await GetUserAsync(principal);

        int? IUserManager.GetUserId(ClaimsPrincipal principal)
        {
            string result = GetUserId(principal);
            if (result == null) return null;
            return int.Parse(result);
        }

        IUser IUserManager.CreateEmpty(string username) => new TUser() { UserName = username };
        #endregion
    }
}
