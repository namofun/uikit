using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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
                  logger,
                  advOptions)
        {
            SlideExpirationStore = slideExpirationService;
        }

        /// <summary>
        /// Gets the expiration for this store.
        /// </summary>
        private SlideExpirationService SlideExpirationStore { get; }

        /// <inheritdoc />
        public override Task<IdentityResult> SlideExpirationAsync(TUser user)
        {
            SlideExpirationStore.Set(user.UserName);
            return Task.FromResult(IdentityResult.Success);
        }
    }
}
