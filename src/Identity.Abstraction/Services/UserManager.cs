using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    /// <summary>
    /// Provides the APIs for managing user in a persistence store.
    /// </summary>
    public interface IUserManager
    {
        /// <summary>
        /// Creates the specified user in the backing store with no password, as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> CreateAsync(IUser user);

        /// <summary>
        /// Creates the specified role in the backing store, as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role to create.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> CreateAsync(IRole role);

        /// <summary>
        /// Finds and returns a user, if any, who has the specified user name.
        /// </summary>
        /// <param name="userName">The user name to search for.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified userName if it exists.</returns>
        Task<IUser> FindByNameAsync(string userName);

        /// <summary>
        /// Finds and returns a role, if any, who has the specified role name.
        /// </summary>
        /// <param name="roleName">The role name to search for.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the role matching the specified roleName if it exists.</returns>
        Task<IRole> FindRoleByNameAsync(string roleName);

        /// <summary>
        /// Finds and returns a user, if any, who has the specified email.
        /// </summary>
        /// <param name="email">The email to search for.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified email if it exists.</returns>
        Task<IUser> FindByEmailAsync(string email);

        /// <summary>
        /// Finds and returns a user, if any, who has the specified user ID.
        /// </summary>
        /// <param name="userId">The user ID to search for.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified userId if it exists.</returns>
        Task<IUser> FindByIdAsync(int userId);

        /// <summary>
        /// Finds and returns a role, if any, who has the specified role ID.
        /// </summary>
        /// <param name="roleId">The role ID to search for.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the role matching the specified roleId if it exists.</returns>
        Task<IRole> FindRoleByIdAsync(int roleId);

        /// <summary>
        /// Gets the paged list of users.
        /// </summary>
        /// <param name="page">The page id.</param>
        /// <param name="pageCount">The count per page.</param>
        /// <returns>The task for fetching user list.</returns>
        Task<IPagedList<IUser>> ListUsersAsync(int page, int pageCount);

        /// <summary>
        /// Finds the user names in the <paramref name="userIds"/>.
        /// </summary>
        /// <param name="userIds">The user IDs.</param>
        /// <returns>The dictionary of user name.</returns>
        Task<Dictionary<int, string>> FindUserNamesAsync(IEnumerable<int> userIds);

        /// <summary>
        /// Updates the specified user in the backing store.
        /// </summary>
        /// <param name="user">The user to update.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> UpdateAsync(IUser user);

        /// <summary>
        /// Deletes the specified user from the backing store.
        /// </summary>
        /// <param name="user">The user to delete.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> DeleteAsync(IUser user);

        /// <summary>
        /// Updates the specified role in the backing store.
        /// </summary>
        /// <param name="role">The user to update.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> UpdateAsync(IRole role);

        /// <summary>
        /// Deletes the specified role from the backing store.
        /// </summary>
        /// <param name="role">The user to delete.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> DeleteAsync(IRole role);

        /// <summary>
        /// Create empty one to continuous creation.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>A brand new <see cref="IUser"/>.</returns>
        IUser CreateEmpty(string username);

        /// <summary>
        /// Create empty one to continuous creation.
        /// </summary>
        /// <param name="roleName">The role name.</param>
        /// <returns>A brand new <see cref="IRole"/>.</returns>
        IRole CreateEmptyRole(string roleName);

        /// <summary>
        /// Gets a flag indicating whether the specified roleName exists.
        /// </summary>
        /// <param name="roleName">The role name whose existence should be checked.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing true if the role name exists, otherwise false.</returns>
        Task<bool> RoleExistsAsync(string roleName);

        /// <summary>
        /// Normalize email for consistent comparisons.
        /// </summary>
        /// <param name="email">The email to normalize.</param>
        /// <returns>A normalized value representing the specified email.</returns>
        string NormalizeEmail(string email);

        /// <summary>
        /// Normalize user or role name for consistent comparisons.
        /// </summary>
        /// <param name="name">The name to normalize.</param>
        /// <returns>A normalized value representing the specified name.</returns>
        string NormalizeName(string name);

        /// <summary>
        /// The <see cref="IdentityOptions"/> used to configure Identity.
        /// </summary>
        IdentityOptions Options { get; }

        /// <summary>
        /// The <see cref="IdentityAdvancedOptions"/> used to configure features.
        /// </summary>
        IdentityAdvancedOptions Features { get; }

        /// <summary>
        /// Returns a <see cref="PasswordVerificationResult"/> indicating the result of a password hash comparison.
        /// </summary>
        /// <param name="user">The user whose password should be verified.</param>
        /// <param name="providedPassword">The password supplied for comparison.</param>
        /// <returns>A <see cref="PasswordVerificationResult"/> indicating the result of a password hash comparison.</returns>
        PasswordVerificationResult VerifyPassword(IUser user, string providedPassword);

        /// <summary>
        /// Returns a flag indicating whether the specified user his locked out, as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user whose locked out status should be retrieved.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, true if the specified user is locked out, otherwise false.</returns>
        Task<bool> IsLockedOutAsync(IUser user);

        /// <summary>
        /// Locks out a user until the specified end date has passed. Setting a end date in the past immediately unlocks a user.
        /// </summary>
        /// <param name="user">The user whose lockout date should be set.</param>
        /// <param name="lockoutEnd">The <see cref="DateTimeOffset"/> after which the user's lockout should end.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> SetLockoutEndDateAsync(IUser user, DateTimeOffset? lockoutEnd);

        /// <summary>
        /// Locks out all users with its ID in the query.
        /// </summary>
        /// <param name="userIds">The user IDs whom should be locked out.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, returning the count of affected users.</returns>
        Task<int> BatchLockOutAsync(IEnumerable<int> userIds);

        /// <summary>
        /// Generates a token for the given user and purpose.
        /// </summary>
        /// <param name="user">The user the token will be for.</param>
        /// <param name="tokenProvider">The provider which will generate the token.</param>
        /// <param name="purpose">The purpose the token will be for.</param>
        /// <returns>The <see cref="Task"/> that represents result of the asynchronous operation, a token for the given user and purpose.</returns>
        Task<string> GenerateUserTokenAsync(IUser user, string tokenProvider, string purpose);

        /// <summary>
        /// Returns a flag indicating whether the specified <paramref name="token"/> is valid for
        /// the given <paramref name="user"/> and <paramref name="purpose"/>.
        /// </summary>
        /// <param name="user">The user to validate the token against.</param>
        /// <param name="tokenProvider">The token provider used to generate the token.</param>
        /// <param name="purpose">The purpose the token should be generated for.</param>
        /// <param name="token">The token to validate</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, returning true if the <paramref name="token"/>
        /// is valid, otherwise false.
        /// </returns>
        Task<bool> VerifyUserTokenAsync(IUser user, string tokenProvider, string purpose, string token);

        /// <summary>
        /// Format the authenticator uri.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="email">The user email.</param>
        /// <param name="unformattedKey">The unformatted key.</param>
        /// <returns>The authenticator uri.</returns>
        string FormatAuthenticatorUri(string userName, string email, string unformattedKey);

        #region External Login

        /// <summary>
        /// Adds an external <see cref="UserLoginInfo"/> to the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to add the login to.</param>
        /// <param name="login">The external <see cref="UserLoginInfo"/> to add to the specified <paramref name="user"/>.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> AddLoginAsync(IUser user, UserLoginInfo login);

        /// <summary>
        /// Retrieves the associated logins for the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user whose associated logins to retrieve.</param>
        /// <returns>The <see cref="Task"/> for the asynchronous operation, containing a list of <see cref="UserLoginInfo"/> for the specified <paramref name="user"/>, if any.</returns>
        Task<IList<UserLoginInfo>> GetLoginsAsync(IUser user);

        /// <summary>
        /// Attempts to remove the provided external login information from the specified <paramref name="user"/>.
        /// and returns a flag indicating whether the removal succeed or not.
        /// </summary>
        /// <param name="user">The user to remove the login information from.</param>
        /// <param name="loginProvider">The login provide whose information should be removed.</param>
        /// <param name="providerKey">The key given by the external login provider for the specified user.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> RemoveLoginAsync(IUser user, string loginProvider, string providerKey);

        /// <summary>
        /// Returns the authenticator key for the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The authenticator key</returns>
        Task<string> GetAuthenticatorKeyAsync(IUser user);

        /// <summary>
        /// Returns recovery code which are still valid for a user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Recovery code which are still valid for a user.</returns>
        Task<string[]> GetRecoveryCodesAsync(IUser user);

        /// <summary>
        /// Sets a flag indicating whether the specified <paramref name="user"/> has two factor authentication enabled or not,
        /// as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user whose two factor authentication enabled status should be set.</param>
        /// <param name="enabled">A flag indicating whether the specified <paramref name="user"/> has two factor authentication enabled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> SetTwoFactorEnabledAsync(IUser user, bool enabled);

        /// <summary>
        /// Resets the authenticator key for the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Whether the user was successfully updated.</returns>
        Task<IdentityResult> ResetAuthenticatorKeyAsync(IUser user);

        /// <summary>
        /// Generates recovery codes for the user, this invalidates any previous recovery codes for the user.
        /// </summary>
        /// <param name="user">The user to generate recovery codes for.</param>
        /// <param name="number">The number of codes to generate.</param>
        /// <returns>The new recovery codes for the user.  Note: there may be less than number returned, as duplicates will be removed.</returns>
        Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(IUser user, int number);

        /// <summary>
        /// Verifies the specified two factor authentication <paramref name="token" /> against the <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user the token is supposed to be for.</param>
        /// <param name="tokenProvider">The provider which will verify the token.</param>
        /// <param name="token">The token to verify.</param>
        /// <returns>The <see cref="Task"/> that represents result of the asynchronous operation, true if the token is valid, otherwise false.</returns>
        Task<bool> VerifyTwoFactorTokenAsync(IUser user, string tokenProvider, string token);

        #endregion

        #region Email

        /// <summary>
        /// Gets a flag indicating whether the email address for the specified user has been verified, true if the email address is verified otherwise false.
        /// </summary>
        /// <param name="user">The user whose email confirmation status should be returned.</param>
        /// <returns>The task object containing the results of the asynchronous operation, a flag indicating whether the email address for the specified user has been confirmed or not.</returns>
        Task<bool> IsEmailConfirmedAsync(IUser user);

        /// <summary>
        /// Generates an email confirmation token for the specified user.
        /// </summary>
        /// <param name="user">The user to generate an email confirmation token for.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, an email confirmation token.</returns>
        Task<string> GenerateEmailConfirmationTokenAsync(IUser user);

        /// <summary>
        /// Validates that an email confirmation token matches the specified user.
        /// </summary>
        /// <param name="user">The user to validate the token against.</param>
        /// <param name="token">The email confirmation token to validate.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> ConfirmEmailAsync(IUser user, string token);

        /// <summary>
        /// Sets the email address for a user.
        /// </summary>
        /// <param name="user">The user whose email should be set.</param>
        /// <param name="email">The email to set.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> SetEmailAsync(IUser user, string email);

        /// <summary>
        /// Gets the mail list of users with subscription.
        /// </summary>
        /// <returns>The task for fetching user list.</returns>
        Task<IReadOnlyList<string>> ListSubscribedEmailsAsync();

        #endregion

        #region Password

        /// <summary>
        /// Creates the specified user in the backing store with given password, as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <param name="password">The password for the user to hash and store.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> CreateAsync(IUser user, string password);

        /// <summary>
        /// Gets a flag indicating whether the specified user has a password.
        /// </summary>
        /// <param name="user">The user to return a flag for, indicating whether they have a password or not.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, returning true if the specified user has a password otherwise false.</returns>
        Task<bool> HasPasswordAsync(IUser user);

        /// <summary>
        /// Adds the password to the specified user only if the user does not already have a password.
        /// </summary>
        /// <param name="user">The user whose password should be set.</param>
        /// <param name="password">The password to set.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> AddPasswordAsync(IUser user, string password);

        /// <summary>
        /// Changes a user's password after confirming the specified <paramref name="currentPassword"/> is correct, as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user whose password should be set.</param>
        /// <param name="currentPassword">The current password to validate before changing.</param>
        /// <param name="newPassword">The new password to set for the specified user.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> ChangePasswordAsync(IUser user, string currentPassword, string newPassword);

        /// <summary>
        /// Generates a password reset token for the specified user, using the configured password reset token provider.
        /// </summary>
        /// <param name="user">The user to generate a password reset token for.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing a password reset token for the specified user.</returns>
        Task<string> GeneratePasswordResetTokenAsync(IUser user);

        /// <summary>
        /// Resets the user's password to the specified <paramref name="newPassword"/> after validating the given password reset token.
        /// </summary>
        /// <param name="user">The user whose password should be reset.</param>
        /// <param name="token">The password reset token to verify.</param>
        /// <param name="newPassword">The new password to set if reset token verification succeeds.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> ResetPasswordAsync(IUser user, string token, string newPassword);

        #endregion

        #region Principal

        /// <summary>
        /// Get the user id of a <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The user id.</returns>
        int? GetUserId(ClaimsPrincipal principal);

        /// <summary>
        /// Get the user name of a <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The user name.</returns>
        string GetUserName(ClaimsPrincipal principal);

        /// <summary>
        /// Get the nick name of a <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The nick name or default user name.</returns>
        string GetNickName(ClaimsPrincipal principal);

        /// <summary>
        /// Returns the user corresponding to the IdentityOptions.ClaimsIdentity.UserIdClaimType claim in the principal or null.
        /// </summary>
        /// <param name="principal">The principal which contains the user id claim.</param>
        /// <returns>The user corresponding to the IdentityOptions.ClaimsIdentity.UserIdClaimType claim in the principal or null.</returns>
        Task<IUser> GetUserAsync(ClaimsPrincipal principal);

        #endregion

        #region Roles

        /// <summary>
        /// Add the specified user to the named role.
        /// </summary>
        /// <param name="user">The user to add to the named role.</param>
        /// <param name="role">The name of the role to add the user to.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> AddToRoleAsync(IUser user, string role);

        /// <summary>
        /// Add the specified user to the named roles.
        /// </summary>
        /// <param name="user">The user to add to the named roles.</param>
        /// <param name="roles">The name of the roles to add the user to.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> AddToRolesAsync(IUser user, IEnumerable<string> roles);

        /// <summary>
        /// Gets a list of role names the specified user belongs to.
        /// </summary>
        /// <param name="user">The user whose role names to retrieve.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing a list of role names.</returns>
        Task<IList<string>> GetRolesAsync(IUser user);

        /// <summary>
        /// Returns a list of users from the user store who are members of the specified roleName.
        /// </summary>
        /// <param name="roleName">The name of the role whose users should be returned.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the result of the asynchronous query, a list of TUsers who are members of the specified role.</returns>
        Task<IReadOnlyList<IUser>> GetUsersInRoleAsync(string roleName);

        /// <summary>
        /// Returns a flag indicating whether the specified user is a member of the given named role.
        /// </summary>
        /// <param name="user">The user whose role membership should be checked.</param>
        /// <param name="role">The name of the role to be checked.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating whether the specified user is a member of the named role.</returns>
        Task<bool> IsInRoleAsync(IUser user, string role);

        /// <summary>
        /// Removes the specified user from the named role.
        /// </summary>
        /// <param name="user">The user to remove from the named role.</param>
        /// <param name="role">The name of the role to remove the user from.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> RemoveFromRoleAsync(IUser user, string role);

        /// <summary>
        /// Removes the specified user from the named roles.
        /// </summary>
        /// <param name="user">The user to remove from the named roles.</param>
        /// <param name="roles">The name of the roles to remove the user from.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> RemoveFromRolesAsync(IUser user, IEnumerable<string> roles);

        /// <summary>
        /// List roles of user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The task for fetching role list.</returns>
        Task<IReadOnlyList<IRole>> ListRolesAsync(IUser user);

        /// <summary>
        /// Gets the paged list of user roles.
        /// </summary>
        /// <param name="minUserId">The minimum user ID.</param>
        /// <param name="maxUserId">The maximum user ID.</param>
        /// <returns>The task for fetching user role list.</returns>
        Task<ILookup<int, int>> ListUserRolesAsync(int minUserId, int maxUserId);

        /// <summary>
        /// Gets the dictionary of named roles.
        /// </summary>
        /// <returns>The task for fetching role dictionary.</returns>
        Task<IReadOnlyDictionary<int, IRole>> ListNamedRolesAsync();

        #endregion
    }
}
