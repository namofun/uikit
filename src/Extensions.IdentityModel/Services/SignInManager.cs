using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    /// <inheritdoc />
    public class SignInManager2<TUser> :
        SignInManager<TUser>, ISignInManager, ICompatibleSignInManager
        where TUser : SatelliteSite.IdentityModule.Entities.User, new()
    {
        /// <inheritdoc />
        public SignInManager2(
            UserManager<TUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IUserClaimsPrincipalFactory<TUser> userClaimsPrincipalFactory,
            IOptions<IdentityOptions> options,
            ILogger<SignInManager<TUser>> logger,
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            IUserConfirmation<TUser> userConfirmation)
            : base(userManager,
                  httpContextAccessor,
                  userClaimsPrincipalFactory,
                  options,
                  logger,
                  authenticationSchemeProvider,
                  userConfirmation)
        {
        }

        /// <inheritdoc cref="ISignInManager.FindUserAsync(string)" />
        protected virtual Task<TUser> FindUserAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return Task.FromResult<TUser>(null);

            // TODO: Contains('\\') -> Domain Login?
            return userName.Contains('@')
                ? UserManager.FindByEmailAsync(userName)
                : UserManager.FindByNameAsync(userName);
        }

        /// <inheritdoc />
        async Task<IUser> ISignInManager.FindUserAsync(string userName) => await FindUserAsync(userName);

        /// <inheritdoc />
        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var user = await FindUserAsync(userName);
            if (user == null) return SignInResult.Failed;
            return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }

        /// <inheritdoc />
        IUserManager ISignInManager.UserManager => (IUserManager)UserManager;

        /// <inheritdoc />
        Task<SignInResult> ISignInManager.PasswordSignInAsync(IUser user, string password, bool isPersistent, bool lockoutOnFailure) => PasswordSignInAsync((TUser)user, password, isPersistent, lockoutOnFailure);

        /// <inheritdoc />
        Task ISignInManager.RefreshSignInAsync(IUser user) => RefreshSignInAsync((TUser)user);

        /// <inheritdoc />
        Task ISignInManager.SignInAsync(IUser user, bool isPersistent, string authenticationMethod) => SignInAsync((TUser)user, isPersistent, authenticationMethod);

        /// <inheritdoc />
        async Task<IUser> ISignInManager.GetTwoFactorAuthenticationUserAsync() => await GetTwoFactorAuthenticationUserAsync();
    }

    /// <summary>
    /// Provides the APIs for user sign in with compatible support.
    /// </summary>
    public interface ICompatibleSignInManager : ISignInManager
    {
        /// <summary>
        /// Configures the redirect URL and user identifier for the specified external login <paramref name="provider"/>.
        /// </summary>
        /// <param name="provider">The provider to configure.</param>
        /// <param name="redirectUrl">The external login URL users should be redirected to during the login flow.</param>
        /// <param name="userId">The current user's identifier, which will be used to provide CSRF protection.</param>
        /// <returns>A configured <see cref="AuthenticationProperties"/>.</returns>
        AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl, string userId = null);

        /// <summary>
        /// Gets the external login information for the current login, as an asynchronous operation.
        /// </summary>
        /// <param name="expectedXsrf">Flag indication whether a Cross Site Request Forgery token was expected in the current request.</param>
        /// <returns>The task object representing the asynchronous operation containing the <see cref="ExternalLoginInfo"/> for the sign-in attempt.</returns>
        Task<ExternalLoginInfo> GetExternalLoginInfoAsync(string expectedXsrf = null);

        /// <summary>
        /// Gets a collection of <see cref="AuthenticationScheme"/>s for the known external login providers.		
        /// </summary>		
        /// <returns>A collection of <see cref="AuthenticationScheme"/>s for the known external login providers.</returns>		
        Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync();
    }

    /// <summary>
    /// Provides the APIs for user sign in with compatible support.
    /// </summary>
    public static class SignInManagerCompatibilityExtensions
    {
        /// <summary>
        /// Configures the redirect URL and user identifier for the specified external login <paramref name="provider"/>.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="provider">The provider to configure.</param>
        /// <param name="redirectUrl">The external login URL users should be redirected to during the login flow.</param>
        /// <param name="userId">The current user's identifier, which will be used to provide CSRF protection.</param>
        /// <returns>A configured <see cref="AuthenticationProperties"/>.</returns>
        public static AuthenticationProperties ConfigureExternalAuthenticationProperties(this ISignInManager signInManager, string provider, string redirectUrl, string userId = null)
        {
            return ((ICompatibleSignInManager)signInManager).ConfigureExternalAuthenticationProperties(provider, redirectUrl, userId);
        }

        /// <summary>
        /// Gets the external login information for the current login, as an asynchronous operation.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="expectedXsrf">Flag indication whether a Cross Site Request Forgery token was expected in the current request.</param>
        /// <returns>The task object representing the asynchronous operation containing the <see cref="ExternalLoginInfo"/> for the sign-in attempt.</returns>
        public static Task<ExternalLoginInfo> GetExternalLoginInfoAsync(this ISignInManager signInManager, string expectedXsrf = null)
        {
            return ((ICompatibleSignInManager)signInManager).GetExternalLoginInfoAsync(expectedXsrf);
        }

        /// <summary>
        /// Gets a collection of <see cref="AuthenticationScheme"/>s for the known external login providers.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <returns>A collection of <see cref="AuthenticationScheme"/>s for the known external login providers.</returns>
        public static Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync(this ISignInManager signInManager)
        {
            return ((ICompatibleSignInManager)signInManager).GetExternalAuthenticationSchemesAsync();
        }
    }
}
