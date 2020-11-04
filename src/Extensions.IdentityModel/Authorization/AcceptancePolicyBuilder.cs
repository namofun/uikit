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
        private List<string> _roles;
        private List<string> _schemes;
        private readonly Dictionary<string, List<string>> _claims;

        /// <summary>
        /// Construct an instance of <see cref="AcceptancePolicyBuilder"/>.
        /// </summary>
        public AcceptancePolicyBuilder()
        {
            _builder = new AuthorizationPolicyBuilder();
            _claims = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Builds a new <see cref="AuthorizationPolicy"/> from the requirements in this instance.
        /// </summary>
        /// <returns>A new <see cref="AuthorizationPolicy"/> built from the requirements in this instance.</returns>
        public AuthorizationPolicy Build()
        {
            if (_roles != null) _builder.RequireRole(_roles);
            if (_schemes != null) _builder.AddAuthenticationSchemes(_schemes.ToArray());

            foreach (var (claimType, acceptValues) in _claims)
            {
                if (acceptValues == null) _builder.RequireClaim(claimType);
                else _builder.RequireClaim(claimType, acceptValues);
            }

            return _builder.Build();
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
        public AcceptancePolicyBuilder AcceptAuthenticationSchemes(params string[] schemes)
        {
            _schemes ??= new List<string>();
            _schemes.AddRange(schemes);
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
        /// Adds the specified requirements to the <see cref="AuthorizationPolicyBuilder.Requirements"/> for this instance.
        /// </summary>
        /// <param name="requirements">The authorization requirements to add.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder AddRequirements(params IAuthorizationRequirement[] requirements)
        {
            _builder.AddRequirements(requirements);
            return this;
        }

        /// <summary>
        /// Combines the specified policy into the current instance.
        /// </summary>
        /// <param name="policyAction">The <see cref="AuthorizationPolicyBuilder"/> to combine.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder Combine(Action<AuthorizationPolicyBuilder> policyAction)
        {
            policyAction(_builder);
            return this;
        }

        /// <summary>
        /// Combines the specified policy into the current instance.
        /// </summary>
        /// <param name="policy">The <see cref="AuthorizationPolicy"/> to combine.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public AcceptancePolicyBuilder Combine(AuthorizationPolicy policy)
        {
            _builder.Combine(policy);
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
    }
}
