﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SatelliteSite.IdentityModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    /// <inheritdoc cref="UserManager{TUser}" />
    public abstract class UserManagerBase<TUser, TRole> :
        UserManager<TUser>, IUserManager
        where TUser : User, new()
        where TRole : Role, new()
    {
        private bool _disposed = false;

        /// <summary>
        /// The store for roles
        /// </summary>
        protected IRoleStore<TRole> RoleStore { get; }

        /// <inheritdoc />
        public IdentityAdvancedOptions Features { get; }

        /// <summary>
        /// Construct a new instance of <see cref="UserManagerBase{TUser,TRole}"/>.
        /// </summary>
        public UserManagerBase(
            IUserStore<TUser> store,
            IRoleStore<TRole> roleStore,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUser> passwordHasher,
            IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManagerBase<TUser, TRole>> logger,
            IOptions<IdentityAdvancedOptions> options2Accessor)
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
            RoleStore = roleStore;
            Features = options2Accessor.Value;
        }

        /// <inheritdoc />
        public override bool SupportsUserTwoFactor => Features.TwoFactorAuthentication;

        /// <inheritdoc />
        public override bool SupportsUserTwoFactorRecoveryCodes => Features.TwoFactorAuthentication;

        /// <inheritdoc />
        public override bool SupportsUserLogin => Features.ExternalLogin;

        /// <inheritdoc />
        public override bool SupportsUserAuthenticatorKey => Features.TwoFactorAuthentication;

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

        #region Slide Expiration

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

        /// <inheritdoc />
        public override async Task<IdentityResult> AddClaimAsync(TUser user, Claim claim)
        {
            var result = await base.AddClaimAsync(user, claim);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <inheritdoc />
        public override async Task<IdentityResult> AddClaimsAsync(TUser user, IEnumerable<Claim> claims)
        {
            var result = await base.AddClaimsAsync(user, claims);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <inheritdoc />
        public override async Task<IdentityResult> RemoveClaimAsync(TUser user, Claim claim)
        {
            var result = await base.RemoveClaimAsync(user, claim);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <inheritdoc />
        public override async Task<IdentityResult> RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims)
        {
            var result = await base.RemoveClaimsAsync(user, claims);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <inheritdoc />
        public override async Task<IdentityResult> ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim)
        {
            var result = await base.ReplaceClaimAsync(user, claim, newClaim);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        #endregion

        #region Relay with IUserListStore<TUser, TRole>

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
        /// Gets the instance of <see cref="IUserListStore{TUser, TRole}"/>.
        /// </summary>
        /// <returns>The store type.</returns>
        private IUserListStore<TUser, TRole> GetListStore()
        {
            var cast = Store as IUserListStore<TUser, TRole>;
            if (cast == null)
            {
                throw new NotSupportedException("The store doesn't implement IUserListStore<TUser, TRole>.");
            }

            return cast;
        }

        /// <inheritdoc cref="IUserManager.ListRolesAsync(IUser)" />
        public virtual Task<List<TRole>> ListRolesAsync(TUser user)
        {
            ThrowIfDisposed();
            return GetListStore().ListRolesAsync(user, CancellationToken);
        }

        /// <inheritdoc cref="IUserManager.ListUsersAsync(int, int)" />
        public virtual Task<IPagedList<TUser>> ListUsersAsync(int page, int pageCount)
        {
            ThrowIfDisposed();
            return GetListStore().ListAsync(page, pageCount, CancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<ILookup<int, int>> ListUserRolesAsync(int minUserId, int maxUserId)
        {
            ThrowIfDisposed();
            return GetListStore().ListUserRolesAsync(minUserId, maxUserId, CancellationToken);
        }

        /// <inheritdoc cref="IUserManager.ListNamedRolesAsync" />
        public virtual Task<List<TRole>> ListNamedRolesAsync()
        {
            ThrowIfDisposed();
            return GetListStore().ListNamedRolesAsync(CancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyList<string>> ListSubscribedEmailsAsync()
        {
            ThrowIfDisposed();
            return await GetListStore().ListSubscribedEmailsAsync(CancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<bool> RoleExistsAsync(string roleName)
        {
            ThrowIfDisposed();
            return GetListStore().ExistRoleAsync(NormalizeName(roleName), CancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<int> BatchLockOutAsync(IEnumerable<int> userIds)
        {
            ThrowIfDisposed();
            return GetListStore().LockOutUsersAsync(userIds, CancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<Dictionary<int, string>> FindUserNamesAsync(IEnumerable<int> userIds)
        {
            ThrowIfDisposed();
            return GetListStore().ListUserNamesAsync(userIds, CancellationToken);
        }

        #endregion

        #region IUser conversion
        Task<IdentityResult> IUserManager.AddToRoleAsync(IUser user, string role) => AddToRoleAsync((TUser)user, role);
        Task<IdentityResult> IUserManager.AddToRolesAsync(IUser user, IEnumerable<string> roles) => AddToRolesAsync((TUser)user, roles);
        Task<bool> IUserManager.IsInRoleAsync(IUser user, string role) => IsInRoleAsync((TUser)user, role);
        Task<IdentityResult> IUserManager.RemoveFromRoleAsync(IUser user, string role) => RemoveFromRoleAsync((TUser)user, role);
        Task<IdentityResult> IUserManager.RemoveFromRolesAsync(IUser user, IEnumerable<string> roles) => RemoveFromRolesAsync((TUser)user, roles);
        Task<IList<string>> IUserManager.GetRolesAsync(IUser user) => GetRolesAsync((TUser)user);
        async Task<IReadOnlyList<IRole>> IUserManager.ListRolesAsync(IUser user) => await ListRolesAsync((TUser)user);
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
        Task<IdentityResult> IUserManager.CreateAsync(IRole role) => RoleStore.CreateAsync((TRole)role, CancellationToken);
        Task<IdentityResult> IUserManager.UpdateAsync(IRole role) => RoleStore.UpdateAsync((TRole)role, CancellationToken);
        Task<IdentityResult> IUserManager.DeleteAsync(IRole role) => RoleStore.DeleteAsync((TRole)role, CancellationToken);
        async Task<IPagedList<IUser>> IUserManager.ListUsersAsync(int page, int pageCount) => await ListUsersAsync(page, pageCount);
        async Task<IReadOnlyDictionary<int, IRole>> IUserManager.ListNamedRolesAsync() => (await ListNamedRolesAsync()).ToDictionary(a => a.Id, a => (IRole)a);
        async Task<IRole> IUserManager.FindRoleByNameAsync(string roleName) => await RoleStore.FindByNameAsync(NormalizeName(roleName), CancellationToken);
        async Task<IRole> IUserManager.FindRoleByIdAsync(int roleId) => await RoleStore.FindByIdAsync($"{roleId}", CancellationToken);
        Task<bool> IUserManager.IsLockedOutAsync(IUser user) => IsLockedOutAsync((TUser)user);
        Task<IdentityResult> IUserManager.SetLockoutEndDateAsync(IUser user, DateTimeOffset? lockoutEnd) => SetLockoutEndDateAsync((TUser)user, lockoutEnd);
        Task<string> IUserManager.GenerateUserTokenAsync(IUser user, string tokenProvider, string purpose) => GenerateUserTokenAsync((TUser)user, tokenProvider, purpose);
        Task<bool> IUserManager.VerifyUserTokenAsync(IUser user, string tokenProvider, string purpose, string token) => VerifyUserTokenAsync((TUser)user, tokenProvider, purpose, token);
        Task<IdentityResult> IUserManager.AddLoginAsync(IUser user, UserLoginInfo login) => AddLoginAsync((TUser)user, login);
        Task<IList<UserLoginInfo>> IUserManager.GetLoginsAsync(IUser user) => GetLoginsAsync((TUser)user);
        Task<IdentityResult> IUserManager.RemoveLoginAsync(IUser user, string loginProvider, string providerKey) => RemoveLoginAsync((TUser)user, loginProvider, providerKey);
        Task<string> IUserManager.GetAuthenticatorKeyAsync(IUser user) => GetAuthenticatorKeyAsync((TUser)user);
        Task<IdentityResult> IUserManager.SetTwoFactorEnabledAsync(IUser user, bool enabled) => SetTwoFactorEnabledAsync((TUser)user, enabled);
        Task<IdentityResult> IUserManager.ResetAuthenticatorKeyAsync(IUser user) => ResetAuthenticatorKeyAsync((TUser)user);
        Task<IEnumerable<string>> IUserManager.GenerateNewTwoFactorRecoveryCodesAsync(IUser user, int number) => GenerateNewTwoFactorRecoveryCodesAsync((TUser)user, number);
        Task<bool> IUserManager.VerifyTwoFactorTokenAsync(IUser user, string tokenProvider, string token) => VerifyTwoFactorTokenAsync((TUser)user, tokenProvider, token);
        Task<string[]> IUserManager.GetRecoveryCodesAsync(IUser user) => GetRecoveryCodesAsync((TUser)user);

        int? IUserManager.GetUserId(ClaimsPrincipal principal)
        {
            string result = GetUserId(principal);
            if (result == null) return null;
            return int.Parse(result);
        }

        PasswordVerificationResult IUserManager.VerifyPassword(IUser user, string providedPassword)
        {
            TUser userEntity = (TUser)user;
            return PasswordHasher.VerifyHashedPassword(userEntity, userEntity.PasswordHash, providedPassword);
        }

        IUser IUserManager.CreateEmpty(string username) => new TUser() { UserName = username };
        IRole IUserManager.CreateEmptyRole(string roleName) => new TRole { Name = roleName, NormalizedName = NormalizeName(roleName) };
        #endregion

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
                RoleStore.Dispose();
            base.Dispose(disposing);
            _disposed = true;
        }

        /// <inheritdoc />
        public virtual async Task<string[]> GetRecoveryCodesAsync(TUser user)
        {
            const string InternalLoginProvider = "[AspNetUserStore]";
            const string RecoveryCodeTokenName = "RecoveryCodes";
            var tokens = await GetAuthenticationTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName);
            if (tokens == null || tokens.Length == 0) return Array.Empty<string>();
            return tokens.Split(';');
        }

        /// <inheritdoc />
        public virtual string FormatAuthenticatorUri(string userName, string email, string unformattedKey)
        {
            var formatter = Features.AuthenticatorUriFormat;
            formatter ??= Features.DefaultFormatter;
            return formatter.Invoke(userName, email, unformattedKey);
        }
    }
}
