using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// Container for advanced authorization building.
    /// </summary>
    public interface IAuthorizationPolicyContainer
    {
        /// <summary>
        /// Add a policy that is built from a delegate with the provided name.
        /// </summary>
        /// <param name="name">The name of the policy.</param>
        /// <param name="configurePolicy">The delegate that will be used to build the policy.</param>
        void AddPolicy2(string name, Action<AcceptancePolicyBuilder> configurePolicy);

        /// <summary>
        /// Add a policy that is built from a delegate with the provided name.
        /// </summary>
        /// <param name="name">The name of the policy.</param>
        /// <param name="configurePolicy">The delegate that will be used to build the policy.</param>
        void AddPolicy(string name, Action<AuthorizationPolicyBuilder> configurePolicy);
    }

    /// <summary>
    /// Container for advanced authorization building.
    /// </summary>
    internal sealed class AuthorizationPolicyContainer : IAuthorizationPolicyContainer
    {
        private readonly Dictionary<string, Action<AuthorizationPolicyBuilder>> _policy1;
        private readonly Dictionary<string, List<Action<AcceptancePolicyBuilder>>> _policy2;

        public AuthorizationPolicyContainer()
        {
            _policy1 = new Dictionary<string, Action<AuthorizationPolicyBuilder>>();
            _policy2 = new Dictionary<string, List<Action<AcceptancePolicyBuilder>>>();
        }

        public void Apply(AuthorizationOptions options)
        {
            foreach (var item in _policy1)
            {
                var builder = new AuthorizationPolicyBuilder();
                item.Value.Invoke(builder);
                options.AddPolicy(item.Key, builder.Build());
            }

            foreach (var item in _policy2)
            {
                var builder = new AcceptancePolicyBuilder();
                item.Value.ForEach(a => a.Invoke(builder));
                options.AddPolicy(item.Key, builder.Build());
            }
        }

        public void AddPolicy(string name, Action<AuthorizationPolicyBuilder> configurePolicy)
        {
            if (_policy2.ContainsKey(name) || _policy1.ContainsKey(name))
                throw new ArgumentException($"Policy {name} duplicated.");
            _policy1.Add(name, configurePolicy ?? throw new ArgumentNullException(nameof(configurePolicy)));
        }

        public void AddPolicy2(string name, Action<AcceptancePolicyBuilder> configurePolicy)
        {
            if (_policy1.ContainsKey(name))
                throw new ArgumentException($"Policy {name} duplicated.");
            if (!_policy2.ContainsKey(name))
                _policy2.Add(name, new List<Action<AcceptancePolicyBuilder>>());
            _policy2[name].Add(configurePolicy);
        }
    }
}
