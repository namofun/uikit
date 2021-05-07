using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.AspNetCore.Authorization
{
    public class AuthorizationPolicyRegistryConfigurator : IConfigureOptions<AuthorizationOptions>
    {
        private readonly ReadOnlyCollection<AbstractModule> _modules;

        public AuthorizationPolicyRegistryConfigurator(ReadOnlyCollection<AbstractModule> modules)
        {
            _modules = modules;
        }

        public void Configure(AuthorizationOptions options)
        {
            var policyContainer = new AuthorizationPolicyContainer();
            foreach (var r in _modules.OfType<IAuthorizationPolicyRegistry>())
                r.RegisterPolicies(policyContainer);
            policyContainer.Apply(options);
        }
    }
}
