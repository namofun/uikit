using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Authorization
{
    /// <inheritdoc />
    public class PartialAuthorizationHandlerContext : AuthorizationHandlerContext
    {
        private bool _anySucceeded;

        /// <summary>
        /// Flag indicating whether the current authorization processing has succeeded.
        /// </summary>
        public virtual bool AnySucceeded => _anySucceeded;

        /// <inheritdoc />
        public override void Succeed(IAuthorizationRequirement requirement)
        {
            base.Succeed(requirement);
            _anySucceeded = true;
        }

        /// <summary>
        /// Creates a new instance of <see cref="PartialAuthorizationHandlerContext"/>.
        /// </summary>
        /// <param name="requirements">A collection of all the <see cref="IAuthorizationRequirement"/> for the current authorization action.</param>
        /// <param name="user">A <see cref="ClaimsPrincipal"/> representing the current user.</param>
        /// <param name="resource">An optional resource to evaluate the requirements against.</param>
        public PartialAuthorizationHandlerContext(
            IEnumerable<IAuthorizationRequirement> requirements,
            ClaimsPrincipal user,
            object? resource)
            : base(requirements, user, resource)
        {
        }
    }
}
