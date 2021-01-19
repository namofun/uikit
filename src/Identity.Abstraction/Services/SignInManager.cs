using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    /// <summary>
    /// Provides the APIs for user sign in.
    /// </summary>
    public interface ISignInManager
    {
        /// <summary>
        /// The <see cref="IUserManager"/> used.
        /// </summary>
        IUserManager UserManager { get; }

        /// <summary>
        /// Signs the current user out of the application.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SignOutAsync();

        /// <summary>
        /// Finds the user with specified <paramref name="userName"/>.
        /// </summary>
        /// <param name="userName">The user name to sign in.</param>
        /// <returns>The task object representing the asynchronous operation containing the <see cref="IUser"/> for the find result.</returns>
        Task<IUser> FindUserAsync(string userName);

        /// <summary>
        /// Attempts to sign in the specified <paramref name="userName"/> and <paramref name="password"/> combination as an asynchronous operation.
        /// </summary>
        /// <param name="userName">The user name to sign in.</param>
        /// <param name="password">The password to attempt to sign in with.</param>
        /// <param name="isPersistent">Flag indicating whether the sign-in cookie should persist after the browser is closed.</param>
        /// <param name="lockoutOnFailure">Flag indicating if the user account should be locked if the sign in fails.</param>
        /// <returns>The task object representing the asynchronous operation containing the <see cref="SignInResult"/> for the sign-in attempt.</returns>
        Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure);

        /// <summary>
        /// Attempts to sign in the specified <paramref name="user"/> and <paramref name="password"/> combination as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user to sign in.</param>
        /// <param name="password">The password to attempt to sign in with.</param>
        /// <param name="isPersistent">Flag indicating whether the sign-in cookie should persist after the browser is closed.</param>
        /// <param name="lockoutOnFailure">Flag indicating if the user account should be locked if the sign in fails.</param>
        /// <returns>The task object representing the asynchronous operation containing the <see cref="SignInResult"/> for the sign-in attempt.</returns>
        Task<SignInResult> PasswordSignInAsync(IUser user, string password, bool isPersistent, bool lockoutOnFailure);

        /// <summary>
        /// Signs in the specified user.
        /// </summary>
        /// <param name="user">The user to sign-in.</param>
        /// <param name="isPersistent">Flag indicating whether the sign-in cookie should persist after the browser is closed.</param>
        /// <param name="authenticationMethod">Name of the method used to authenticate the user.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SignInAsync(IUser user, bool isPersistent, string authenticationMethod = null);

        /// <summary>
        /// Regenerates the user's application cookie, whilst preserving the existing AuthenticationProperties like rememberMe, as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user whose sign-in cookie should be refreshed.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task RefreshSignInAsync(IUser user);

        /// <summary>
        /// Gets the <see cref="IUser"/> for the current two factor authentication login, as an asynchronous operation.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation containing the <see cref="IUser"/> for the sign-in attempt.</returns>
        Task<IUser> GetTwoFactorAuthenticationUserAsync();

        /// <summary>
        /// Validates the sign in code from an authenticator app and creates and signs in the user, as an asynchronous operation.
        /// </summary>
        /// <param name="code">The two factor authentication code to validate.</param>
        /// <param name="isPersistent">Flag indicating whether the sign-in cookie should persist after the browser is closed.</param>
        /// <param name="rememberClient">Flag indicating whether the current browser should be remember, suppressing all further two factor authentication prompts.</param>
        /// <returns>The task object representing the asynchronous operation containing the <see cref="SignInResult"/> for the sign-in attempt.</returns>
        Task<SignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool isPersistent, bool rememberClient);

        /// <summary>
        /// Signs in the user without two factor authentication using a two factor recovery code.
        /// </summary>
        /// <param name="recoveryCode">The two factor recovery code.</param>
        /// <returns>The task object representing the asynchronous operation containing the <see cref="SignInResult"/> for the sign-in attempt.</returns>
        Task<SignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode);

        /// <summary>
        /// Signs in a user via a previously registered third party login, as an asynchronous operation.
        /// </summary>
        /// <param name="loginProvider">The login provider to use.</param>
        /// <param name="providerKey">The unique provider identifier for the user.</param>
        /// <param name="isPersistent">Flag indicating whether the sign-in cookie should persist after the browser is closed.</param>
        /// <param name="bypassTwoFactor">Flag indicating whether to bypass two factor authentication.</param>
        /// <returns>The task object representing the asynchronous operation containing the <see name="SignInResult"/>
        /// for the sign-in attempt.</returns>
        Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor = false);
    }
}
