using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// Container for advanced authorization building.
    /// </summary>
    public class AcceptancePolicyBuilder
    {
        private readonly AuthorizationPolicyBuilder _builder;
        private readonly Dictionary<string, List<string>> _claims;
        private readonly List<IAuthorizationRequirement> _requirements;
        private List<string> _roles;

        /// <summary>
        /// Construct an instance of <see cref="AcceptancePolicyBuilder"/>.
        /// </summary>
        public AcceptancePolicyBuilder()
        {
            _requirements = new List<IAuthorizationRequirement>();
            _builder = new AuthorizationPolicyBuilder();
            _claims = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Builds a new <see cref="AuthorizationPolicy"/> from the requirements in this instance.
        /// </summary>
        /// <returns>A new <see cref="AuthorizationPolicy"/> built from the requirements in this instance.</returns>
        public AuthorizationPolicy Build()
        {
            var partialRequirements = new List<IAuthorizationRequirement>(_requirements);

            if (_roles != null)
            {
                partialRequirements.Add(new Infrastructure.RolesAuthorizationRequirement(_roles));
            }

            foreach (var (claimType, acceptValues) in _claims)
            {
                partialRequirements.Add(new Infrastructure.ClaimsAuthorizationRequirement(claimType, acceptValues));
            }

            if (partialRequirements.Count == 1)
            {
                _builder.AddRequirements(partialRequirements[0]);
            }
            else if (partialRequirements.Count > 1)
            {
                _builder.AddRequirements(new Infrastructure.AcceptanceRequirement(partialRequirements));
            }

            return _builder.Build();
        }

        /// <summary>
        /// Adds a <see cref="Infrastructure.DenyAnonymousAuthorizationRequirement"/> to the current instance.
        /// </summary>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder RequireAuthenticatedUser()
        {
            _builder.RequireAuthenticatedUser();
            return this;
        }

        /// <summary>
        /// Adds the specified authentication schemes to the <see cref="AuthorizationPolicyBuilder.AuthenticationSchemes"/> for this instance.
        /// </summary>
        /// <param name="schemes">The schemes to add.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder AddAuthenticationSchemes(params string[] schemes)
        {
            _builder.AddAuthenticationSchemes(schemes);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="Infrastructure.NameAuthorizationRequirement"/> to the current instance.
        /// </summary>
        /// <param name="userName">The user name the current user must possess.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder RequireAuthenticatedUser(string userName)
        {
            _builder.RequireUserName(userName);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="Infrastructure.NameAuthorizationRequirement"/> to the current instance.
        /// </summary>
        /// <param name="userName">The user name the current user must possess.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder AcceptAuthenticatedUser(string userName)
        {
            _requirements.Add(new Infrastructure.NameAuthorizationRequirement(userName));
            return this;
        }

        /// <summary>
        /// Adds a <see cref="Infrastructure.ClaimsAuthorizationRequirement"/> to the current instance.
        /// </summary>
        /// <param name="claimType">The claim type required, which no restrictions on claim value.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder RequireClaim(string claimType)
        {
            _builder.RequireClaim(claimType);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="Infrastructure.ClaimsAuthorizationRequirement"/> to the current instance.
        /// </summary>
        /// <param name="claimType">The claim type required.</param>
        /// <param name="allowedValues">Values the claim must process one or more of for evaluation to succeed.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder RequireClaim(string claimType, params string[] allowedValues)
        {
            _builder.RequireClaim(claimType, allowedValues);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="Infrastructure.ClaimsAuthorizationRequirement"/> to the current instance.
        /// </summary>
        /// <param name="claimType">The claim type required, which no restrictions on claim value.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder AcceptClaim(string claimType)
        {
            _claims[claimType] = null;
            return this;
        }

        /// <summary>
        /// Adds a <see cref="Infrastructure.ClaimsAuthorizationRequirement"/> to the current instance.
        /// </summary>
        /// <param name="claimType">The claim type required.</param>
        /// <param name="allowedValues">Values the claim must process one or more of for evaluation to succeed.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder AcceptClaim(string claimType, params string[] allowedValues)
        {
            if (!_claims.ContainsKey(claimType)) _claims.Add(claimType, new List<string>());
            _claims[claimType]?.AddRange(allowedValues);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="Infrastructure.RolesAuthorizationRequirement"/> to the current instance.
        /// </summary>
        /// <param name="roles">The allowed roles.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder RequireRole(params string[] roles)
        {
            _builder.RequireRole(roles);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="Infrastructure.RolesAuthorizationRequirement"/> to the current instance.
        /// </summary>
        /// <param name="roles">The allowed roles.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder AcceptRole(params string[] roles)
        {
            _roles ??= new List<string>();
            _roles.AddRange(roles);
            return this;
        }

        /// <summary>
        /// Adds an <see cref="Infrastructure.AssertionRequirement"/> to the current instance.
        /// </summary>
        /// <param name="handler">The handler to evaluate during authorization.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder RequireAssertion(Func<AuthorizationHandlerContext, bool> handler)
        {
            _builder.RequireAssertion(handler);
            return this;
        }

        /// <summary>
        /// Adds an <see cref="Infrastructure.AssertionRequirement"/> to the current instance.
        /// </summary>
        /// <param name="handler">The handler to evaluate during authorization.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder RequireAssertion(Func<AuthorizationHandlerContext, Task<bool>> handler)
        {
            _builder.RequireAssertion(handler);
            return this;
        }

        /// <summary>
        /// Adds an <see cref="Infrastructure.AssertionRequirement"/> to the current instance.
        /// </summary>
        /// <param name="handler">The handler to evaluate during authorization.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder AcceptAssertion(Func<AuthorizationHandlerContext, bool> handler)
        {
            _requirements.Add(new Infrastructure.AssertionRequirement(handler));
            return this;
        }

        /// <summary>
        /// Adds an <see cref="Infrastructure.AssertionRequirement"/> to the current instance.
        /// </summary>
        /// <param name="handler">The handler to evaluate during authorization.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder AcceptAssertion(Func<AuthorizationHandlerContext, Task<bool>> handler)
        {
            _requirements.Add(new Infrastructure.AssertionRequirement(handler));
            return this;
        }

        /// <summary>
        /// Adds the specified requirements to the <see cref="AuthorizationPolicyBuilder.Requirements"/> for this instance.
        /// </summary>
        /// <param name="requirements">The authorization requirements to add.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder RequireRequirements(params IAuthorizationRequirement[] requirements)
        {
            _builder.AddRequirements(requirements);
            return this;
        }

        /// <summary>
        /// Adds the specified requirements to the <see cref="AuthorizationPolicyBuilder.Requirements"/> for this instance.
        /// </summary>
        /// <param name="requirements">The authorization requirements to add.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder AcceptRequirements(params IAuthorizationRequirement[] requirements)
        {
            _requirements.AddRange(requirements);
            return this;
        }
    }
}
