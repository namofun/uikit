using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace SatelliteSite.IdentityModule.Services
{
    /// <inheritdoc />
    public class SignInManager2<TUser> :
        SignInManager<TUser>, ISignInManager
        where TUser : Entities.User, new()
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

        /// <inheritdoc />
        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            TUser user;
            if (string.IsNullOrEmpty(userName)) return SignInResult.Failed;

            // TODO: Contains('\\') -> Domain Login?
            user = userName.Contains('@')
                ? await UserManager.FindByEmailAsync(userName)
                : await UserManager.FindByNameAsync(userName);

            if (user == null) return SignInResult.Failed;
            return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }

        /// <inheritdoc />
        Task ISignInManager.RefreshSignInAsync(IUser user) => RefreshSignInAsync((TUser)user);

        /// <inheritdoc />
        Task ISignInManager.SignInAsync(IUser user, bool isPersistent, string authenticationMethod) => SignInAsync((TUser)user, isPersistent, authenticationMethod);
    }
}
