using Microsoft.AspNetCore.Authorization;

namespace SatelliteSite
{
    /// <summary>
    /// The provider interface to build policies.
    /// </summary>
    public interface IAuthorizationPolicyRegistry
    {
        /// <summary>
        /// Register authorization policies to the container.
        /// </summary>
        /// <param name="container">The container.</param>
        void RegisterPolicies(IAuthorizationPolicyContainer container);
    }
}
