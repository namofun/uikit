using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    /// <inheritdoc />
    public interface ILightweightUserClaimsPrincipalFactory<TUser> :
        IUserClaimsPrincipalFactory<TUser>
        where TUser : class
    {
    }

    /// <summary>
    /// Provides an abstraction for adding more claims to the provider.
    /// </summary>
    /// <remarks>This should be registered as <see cref="ServiceLifetime.Scoped"/> and multiple instances are allowed.</remarks>
    public interface IUserClaimsProvider
    {
        /// <summary>
        /// Gets claims for such user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The task for creating claims.</returns>
        Task<IEnumerable<Claim>> GetClaimsAsync(IUser user);
    }
}
