using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    /// <inheritdoc />
    public class SignInManager2<TUser> :
        SignInManager<TUser>, ISignInManager
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
        Task<SignInResult> ISignInManager.PasswordSignInAsync(IUser user, string password, bool isPersistent, bool lockoutOnFailure) => PasswordSignInAsync((TUser)user, password, isPersistent, lockoutOnFailure);

        /// <inheritdoc />
        Task ISignInManager.RefreshSignInAsync(IUser user) => RefreshSignInAsync((TUser)user);

        /// <inheritdoc />
        Task ISignInManager.SignInAsync(IUser user, bool isPersistent, string authenticationMethod) => SignInAsync((TUser)user, isPersistent, authenticationMethod);
    }
}
