using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    /// <summary>
    /// Provides methods to create a full claims principal for a given user.
    /// Intended to replace <see cref="UserClaimsPrincipalFactory{TUser, TRole}"/>.
    /// </summary>
    /// <typeparam name="TUser">The type used to represent a user.</typeparam>
    /// <typeparam name="TRole">The type used to represent a role.</typeparam>
    public class FullUserClaimsPrincipalFactory<TUser, TRole> :
        UserClaimsPrincipalFactory<TUser>,
        ILightweightUserClaimsPrincipalFactory<TUser>
        where TUser : SatelliteSite.IdentityModule.Entities.User, new()
        where TRole : SatelliteSite.IdentityModule.Entities.Role, new()
    {
        /// <inheritdoc cref="UserClaimsPrincipalFactory{TUser}.UserManager" />
        public new UserManager<TUser, TRole> UserManager => (UserManager<TUser, TRole>)base.UserManager;

        /// <summary>The external claims provider</summary>
        public CompositeUserClaimsProvider ClaimsProvider { get; }

        /// <summary>
        /// Initializes a new instance of the Microsoft.AspNetCore.Identity.UserClaimsPrincipalFactory`1 class.
        /// </summary>
        /// <param name="userManager">The <see cref="UserManager{TUser,TRole}"/> to retrieve user information from.</param>
        /// <param name="options">The configured <see cref="IdentityOptions"/>.</param>
        /// <param name="claimsProvider">The external <see cref="IUserClaimsProvider"/>s.</param>
        public FullUserClaimsPrincipalFactory(
            UserManager<TUser, TRole> userManager,
            IOptions<IdentityOptions> options,
            CompositeUserClaimsProvider claimsProvider)
            : base(userManager, options)
        {
            ClaimsProvider = claimsProvider;
        }

        /// <inheritdoc />
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TUser user)
        {
            var content = await base.GenerateClaimsAsync(user);

            var roles = await UserManager.GetRolesAsync(user);
            content.AddClaims(roles.Select(roleName => new Claim(Options.ClaimsIdentity.RoleClaimType, roleName)));

            var roleClaims = await UserManager.GetRoleClaimsAsync(user);
            content.AddClaims(roleClaims);

            if (!string.IsNullOrWhiteSpace(user.NickName))
                content.AddClaim(new Claim("nickname", user.NickName));

            if (user.EmailConfirmed)
                content.AddClaim(new Claim("email_verified", "true"));

            await ClaimsProvider.ExecuteAsync(user, content);

            return content;
        }
    }


    /// <summary>
    /// Provides methods to create a lightweight claims principal for a given user.
    /// </summary>
    /// <typeparam name="TUser">The type used to represent a user.</typeparam>
    /// <typeparam name="TRole">The type used to represent a role.</typeparam>
    public class LightweightUserClaimsPrincipalFactory<TUser, TRole> :
        ILightweightUserClaimsPrincipalFactory<TUser>
        where TUser : SatelliteSite.IdentityModule.Entities.User, new()
        where TRole : SatelliteSite.IdentityModule.Entities.Role, new()
    {
        /// <summary>
        /// The user manager used to access user information.
        /// </summary>
        public UserManager<TUser, TRole> UserManager { get; }

        /// <summary>
        /// The identity options used to configure identity.
        /// </summary>
        public IdentityOptions Options => UserManager.Options;

        /// <summary>
        /// Initialize the <see cref="LightweightUserClaimsPrincipalFactory{TUser, TRole}"/>.
        /// </summary>
        /// <param name="userManager">The <see cref="UserManager{TUser, TRole}"/>.</param>
        public LightweightUserClaimsPrincipalFactory(UserManager<TUser, TRole> userManager)
        {
            UserManager = userManager;
        }

        /// <inheritdoc />
        public async Task<ClaimsPrincipal> CreateAsync(TUser user)
        {
            var ur = await UserManager.GetRolesAsync(user);

            var id = new ClaimsIdentity(
                IdentityConstants.ApplicationScheme,
                Options.ClaimsIdentity.UserNameClaimType,
                Options.ClaimsIdentity.RoleClaimType);

            id.AddClaim(new Claim(Options.ClaimsIdentity.UserIdClaimType, $"{user.Id}"));
            id.AddClaim(new Claim(Options.ClaimsIdentity.UserNameClaimType, user.UserName));
            id.AddClaim(new Claim(Options.ClaimsIdentity.SecurityStampClaimType, user.SecurityStamp));
            id.AddClaims(ur.Select(r => new Claim(Options.ClaimsIdentity.RoleClaimType, r)));
            return new ClaimsPrincipal(id);
        }
    }


    /// <summary>
    /// Compose providers together as a lazy service.
    /// </summary>
    public class CompositeUserClaimsProvider : Lazy<IEnumerable<IUserClaimsProvider>>
    {
        /// <summary>
        /// Instantiate the <see cref="CompositeUserClaimsProvider"/>.
        /// </summary>
        /// <param name="sp">The service provider to take from.</param>
        public CompositeUserClaimsProvider(IServiceProvider sp)
            : base(sp.GetServices<IUserClaimsProvider>)
        {
        }

        /// <summary>
        /// Gets claims for such user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="identity">The claims identity.</param>
        /// <returns>The task for creating claims.</returns>
        public async Task ExecuteAsync(IUser user, ClaimsIdentity identity)
        {
            foreach (var provider in Value)
            {
                var claims = await provider.GetClaimsAsync(user);
                identity.AddClaims(claims);
            }
        }
    }
}
