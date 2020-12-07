using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Linq;

namespace SatelliteSite.IdentityModule
{
    internal class ConfigureAuthoraztionPolicy : IConfigureOptions<AuthorizationOptions>
    {
        private readonly ReadOnlyCollection<AbstractModule> _modules;

        public ConfigureAuthoraztionPolicy(ReadOnlyCollection<AbstractModule> modules)
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
