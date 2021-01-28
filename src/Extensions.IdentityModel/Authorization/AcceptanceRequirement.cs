using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authorization.Infrastructure
{
    /// <summary>
    /// Represents an authorization requirement that succeed for any of sub requirements.
    /// </summary>
    public class AcceptanceRequirement : IAuthorizationRequirement, IAuthorizationHandler
    {
        /// <summary>
        /// Creates a new instance of <see cref="AcceptanceRequirement"/>.
        /// </summary>
        /// <param name="requirements">A collection of all the <see cref="IAuthorizationRequirement"/> for the current authorization action.</param>
        public AcceptanceRequirement(IReadOnlyList<IAuthorizationRequirement> requirements)
        {
            Requirements = requirements;
            if (requirements.Any(a => !(a is IAuthorizationHandler)))
                throw new InvalidOperationException(
                    "There's at lease one IAuthorizationRequirement doesn't implement IAuthorizationHandler.");
        }

        /// <summary>
        /// Gets a readonly list of <see cref="IAuthorizationRequirement"/>s which must succeed one for this requirment to be OK.
        /// </summary>
        public IReadOnlyList<IAuthorizationRequirement> Requirements { get; }

        /// <inheritdoc />
        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            var newContext = new PartialAuthorizationHandlerContext(Requirements, context.User, context.Resource);
            for (int i = 0; i < Requirements.Count; i++)
            {
                var requirement = (IAuthorizationHandler)Requirements[i];
                await requirement.HandleAsync(newContext);

                if (newContext.AnySucceeded)
                {
                    context.Succeed(this);
                    return;
                }
            }
        }
    }
}
